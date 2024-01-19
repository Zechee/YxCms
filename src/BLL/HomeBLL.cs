using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using ChuntianCms.Models;
using Microsoft.AspNetCore.Http;

namespace ChuntianCms.BLL
{

    public class HomeBLL
    {

        public static SysUserBLL sysUserBLL = new SysUserBLL();
        public static SysRoleBLL sysRoleBLL = new SysRoleBLL();
        public static SysMenuBLL sysMenuBLL = new SysMenuBLL();
        public static SysRightBLL sysRightBLL = new SysRightBLL();
        public static SiteNewsBLL siteNewsBLL = new SiteNewsBLL();
        public static SiteTypeBLL siteTypeBLL = new SiteTypeBLL();
        public static StudentBLL studentBLL = new StudentBLL();
        
        #region 获取当前用户
        public static async Task<AccountModel> GetCurrentUser(HttpContext context)
        {
            string username = await AuthCookie.GetUserName(context);
            string password = await AuthCookie.GetPassWord(context);
            //string extime = AuthCookie.GetUserExpiresTime();
            var user = sysUserBLL.LoginUser(username, password);
            var account = new AccountModel();
            if (user != null)
            {
                account.UserName = username;
                account.Id = user.Id;
                account.RoleId = user.RoleId;
                account.RoleName = sysRoleBLL.GetById(user.RoleId.ToString()).Name;
                account.TrueName = user.TrueName;
                //account.ExpiresTime = extime;
                account.Logo = user.Logo;
                var ip = Method.GetRealIP(context);
                account.LocalIp = ip;
                return account;
            }
            return null;
        }
        #endregion

        #region 获取菜单
        public static List<SysMenu> GetMenusByRoleId(int roleid)
        {
            var queryData =
             (from m in sysMenuBLL.Lists
              join rl in sysRightBLL.Lists
             on m.Id equals rl.ModuleId
              where rl.Rightflag == true & rl.RoleId == roleid & m.ParentId != -1 && m.IsMenu == 1
              select m).Distinct().OrderBy(a => a.ParentId).ThenBy(a => a.Sort).ToList();
            return queryData;
        }
        #endregion

        #region 获取权限
        public static List<PermModel> GetPermission(int roleid, string controller)
        {
            var menuModel = sysMenuBLL.Lists.Where(a => IsContainController(a.MenuUrl, controller)).FirstOrDefault();
            int menuid = menuModel == null ? 0 : menuModel.Id;
            var menus = from sm in sysMenuBLL.Lists where sm.ParentId == menuid select sm;
            var rights = from sr in sysRightBLL.Lists
                         where sr.RoleId == roleid & menus.Select(a => a.Id).Contains(sr.ModuleId) & sr.Rightflag == true
                         select sr;
            var result = (from sr in rights
                          join sm in menus on sr.ModuleId equals sm.Id
                          select new PermModel
                          {
                              KeyCode = sm.Code
                          }).ToList();

            return result;
        }
        private static bool IsContainController(string url, string ctr)
        {
            url = url ?? "";
            return url.ToLower().IndexOf(ctr.ToLower()) != -1;
        }
        #endregion

        #region 文件上传
        public static string SysUserUploadFile(HttpContext context, string dirpath)
        {
            //接收文件file
            //注意：不要加if(!IsPostBack)判断！
            //file1这个是上面设置FormData的参数名，必须一致！其中Request.Files[]是文件相关的在里面
            //Request["id"]是普通类型参数在里面，例如：int string ...
            var file = context.Request.Form.Files["file"];
            var newid = Method.NewId;
            var filename = context.Request.Form["filename"];
            var imgext = Path.GetExtension(filename);
            string newname = newid + imgext;
            string res = "";
            if (file != null)
            {
                string filepath = Method.GetServerPath(context, dirpath + newname);
                var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                try
                {
                    //fs.Write(dataOne, 0, dataOne.Length);
                    file.CopyTo(fs);
                    res = newname;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    fs.Close();
                }
            }
            return res;
        }
        #endregion

        #region 获取系统配置
        public static bool SetSystemSetting(HttpContext context,SystemSettingViewModel model)
        {
            var jsonStr = JsonConvert.SerializeObject(model);
            var dir = Method.GetServerPath(context, "./App_Data/json/");
            if(!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            var path = Method.GetServerPath(context, "./App_Data/json/" + "SystemSetting.json");
            File.WriteAllText(path, jsonStr);
            return true;

        }
        public static SystemSettingViewModel? GetSystemSetting(HttpContext context)
        {
            string path = Method.GetServerPath(context, "./App_Data/json/" + "SystemSetting.json");
            var jsonStr = File.ReadAllText(path);
            var model = JsonConvert.DeserializeObject<SystemSettingViewModel>(jsonStr);
            return model;
        }
        #endregion

        #region 获取报表数据
        public static async Task<SystemViewModel> GetSystemData(HttpContext context)
        {
            var model = new SystemViewModel();
            var user = await GetCurrentUser(context);
            model.UserName = user.UserName;
            model.RoleName = user.RoleName;
            model.TrueName = user.TrueName;
            model.ServerIp = context.GetServerVariable("Local_Addr")?.ToString() ?? "";
            model.LocalIp = user.LocalIp;
            model.ExpiresTime = user.ExpiresTime;
            return model;
        }
        public static SiteDataViewModel GetSiteData()
        {
            var model = new SiteDataViewModel();
            model.ArticleCount = 100;
            model.ProCount = 100;
            model.SiteCount = 100;
            model.UserCount = sysUserBLL.Lists.Count();
            return model;
        }
        public dynamic GetUserSaleCountColumnChart()
        {

            var typenames = GetTypeNames();
            var srcList = GetIpCountList(typenames);

            SortedList<string, int> seriesList = new SortedList<string, int>();

            var dicNames = typenames.Distinct();

            foreach (string name in dicNames)
            {
                int value = srcList.Where(a => a.TypeName == name).Count();

                seriesList.Add(name, value);

            }
            var list = seriesList.OrderBy(a => a.Value).ToDictionary(a => a.Key);
            
            var option = new
            {
                key = list.Keys,
                series = list.Values.Select(a => a.Value)
            };
            return option;

        }
        public dynamic GetIpCountPieChart()
        {

            var typenames = GetTypeNames();
            var srcList = GetIpCountList(typenames);

            List<dynamic> seriesList = new List<dynamic>();

            var dicNames = typenames.Distinct();
            foreach (string name in dicNames)
            {
                int value = srcList.Where(a => a.TypeName == name).Count();
                var series1 = new
                {
                    name,
                    value
                };
                seriesList.Add(series1);
            }
            var option = new
            {
                dicNames,
                series = seriesList
            };
            return option;

        }

        public dynamic GetIpCountOfTypeNames(int dayCount = 7)
        {

            var typenames = GetTypeNames();
            var srcList = GetIpCountList(typenames);
            List<string> dates = GetAllDate(dayCount);
            List<dynamic> seriesList = new List<dynamic>();

            var dicNames = typenames.Distinct();
            foreach (string name in dicNames)
            {
                var series1 = new
                {
                    name,
                    type = "line",
                    //stack = "总量",
                    areaStyle = new { },
                    data = GetEveryDayCount(srcList, name, dates)
                };
                seriesList.Add(series1);
            }
            var option = new
            {
                dicNames,
                dates,
                series = seriesList
            };
            return option;

        }
        private string[] GetTypeNames()
        {
            string[] typenames = { "邮件营销", "邮件营销", "邮件营销", "搜索引擎", "搜索引擎", "直接访问" };
            return typenames;
        }
        private List<decimal?> GetEveryDayCount(List<IpCountViewModel> srcList, string name, List<string> dates)
        {
            List<decimal?> counts = new List<decimal?>();
            foreach (string date in dates)
            {
                var model = srcList.Where(a => a.TypeName == name && a.CreatedTime.Date.ToLongDateString() == date).FirstOrDefault();
                int count = model == null ? 0 : model.Count;
                counts.Add(count);
            }
            return counts;
        }
        private List<string> GetAllDate(int dayCount)
        {
            DateTime dtNow = DateTime.Now;
            List<string> list = new List<string>();
            for (int i = dayCount; i >= 0; i--)
            {
                var dt = dtNow.AddDays(-i);
                list.Add(dt.Date.ToLongDateString());
            }
            return list;
        }
        private static List<IpCountViewModel> GetIpCountList(string[] typenames)
        {
            int maxDay = 365 * 3;
            List<IpCountViewModel> list = new List<IpCountViewModel>();
            Random rd = new Random();
            DateTime dtNow = DateTime.Now;

            for (int k = 0; k < 3; k++)
            {
                for (int i = maxDay; i >= 0; i--)
                {
                    var dt = dtNow.AddDays(-i);
                    var model = new IpCountViewModel();
                    model.Count = rd.Next(0, 100);
                    var typeIndex = rd.Next(0, typenames.Length);
                    model.TypeName = typenames[typeIndex];
                    model.NewId = Method.NewId;
                    model.CreatedTime = dt;
                    list.Add(model);
                }
            }

            return list;
        }

        #endregion
    }

  
    #region 菜单实体类
    /// <summary>
    /// 菜单结果对象
    /// </summary>
    public class MenusInfoResultDTO
    {
        /// <summary>
        /// 权限菜单树
        /// </summary>
        public List<SystemMenu> menuInfo { get; set; }

        /// <summary>
        /// logo
        /// </summary>
        public LogoInfo logoInfo { get; set; }

        /// <summary>
        /// Home
        /// </summary>
        public HomeInfo homeInfo { get; set; }
    }
    public class LogoInfo
    {
        public string title { get; set; } = "";
        public string image { get; set; } = "/content/images/yutianlogo.png";
        public string href { get; set; } = "";
    }

    public class HomeInfo
    {
        public string title { get; set; } = "仪表板";
        public string href { get; set; } = "/Home/Page1";

    }
    /// <summary>
    /// 树结构对象
    /// </summary>
    public class SystemMenu
    {
        /// <summary>
        /// 数据ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 父级ID
        /// </summary>
        public long PId { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 节点地址
        /// </summary>
        public string href { get; set; }

        /// <summary>
        /// 新开Tab方式
        /// </summary>
        public string target { get; set; } = "_self";

        /// <summary>
        /// 菜单图标样式
        /// </summary>
        public string icon { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 子集
        /// </summary>
        public List<SystemMenu> child { get; set; }
    }

    #endregion


}




