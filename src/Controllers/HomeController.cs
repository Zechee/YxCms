using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChuntianCms.BLL;
using ChuntianCms.Models;
using System.IO;

namespace ChuntianCms.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        SysUserBLL sysUserBLL = HomeBLL.sysUserBLL;
        ErrorMsg errors = new ErrorMsg();
        
        #region 查询
        public async Task<ActionResult> Index()
        {
            var account = await GetAccount();

            if (account != null)
            {
                
                return View(account);
            }
            else
            {
                return Redirect("/Account/Index");

            }
        }
        public ActionResult Page1()
        {
            return View();
        }
        public ActionResult Error()
        {
            return View();
        }
        public ActionResult Icon()
        {
            return View();
        }

        #endregion

        #region 其它
        [HttpGet]
        public JsonResult LoginOut()
        {
            //AuthCookie.LoginOut();
            //int flag = AuthCookie.IsLogin() == true ? 1 : 0;
            var flag = 1;
            return Json(flag);
        }
        [HttpGet]
        public JsonResult Clear()
        {
            dynamic res = new { code = 1, msg = "服务端清理缓存成功" };
            return Json(res);
        }
        #endregion

        #region 菜单
        [HttpGet]
        public async Task<JsonResult> GetAllTreeMenu()
        {
            List<SysMenu> list = new List<SysMenu>();
            AccountModel account = await GetAccount();
            if (account != null)
            {
                list = HomeBLL.GetMenusByRoleId(account.RoleId);
            }

            return Json(list);
        }
        [HttpGet]
        public async Task<JsonResult> SystemMenu()
        {
            List<SysMenu> list = new List<SysMenu>();
            AccountModel account = await GetAccount();
            if (account != null)
            {
                list = HomeBLL.GetMenusByRoleId(account.RoleId);
            }

            SystemMenu rootNode = new SystemMenu()
            {
                Id = 1,
                icon = "",
                href = "",
                title = "系统管理",
            };

            GetTreeNodeListByNoLockedDTOArray(list, rootNode);

            List<SystemMenu> menuInfo = new List<SystemMenu>();
            menuInfo.Add(rootNode);
            MenusInfoResultDTO menusInfoResultDTO = new MenusInfoResultDTO();
            menusInfoResultDTO.menuInfo = menuInfo;
            menusInfoResultDTO.logoInfo = new LogoInfo();
            menusInfoResultDTO.homeInfo = new HomeInfo();

            return Json(menusInfoResultDTO);
        }
        /// <summary>
        /// 递归处理数据
        /// </summary>
        /// <param name="systemMenuEntities"></param>
        /// <param name="rootNode"></param>
        public static void GetTreeNodeListByNoLockedDTOArray(List<SysMenu> systemMenuEntities, SystemMenu rootNode)
        {
            if (systemMenuEntities == null || systemMenuEntities.Count() <= 0)
            {
                return;
            }

            var childreDataList = systemMenuEntities.Where(p => p.ParentId == rootNode.Id);
            if (childreDataList != null && childreDataList.Count() > 0)
            {
                rootNode.child = new List<SystemMenu>();

                foreach (var item in childreDataList)
                {
                    SystemMenu treeNode = new SystemMenu()
                    {
                        Id = item.Id,
                        icon = item.MenuIcon,
                        href = item.MenuUrl,
                        title = item.Name,
                    };
                    rootNode.child.Add(treeNode);
                }

                foreach (var item in rootNode.child)
                {
                    GetTreeNodeListByNoLockedDTOArray(systemMenuEntities, item);
                }
            }
        }


        #endregion

        #region 用户设置
        public async Task<ActionResult> UserSetting()
        {
            AccountModel account = await GetAccount();
            if (account != null)
            {
                return View(account);
            }
            return Redirect("/");
        }
        [HttpPost]
        public JsonResult UserSetting(AccountModel account)
        {
            var model = HomeBLL.sysUserBLL.GetById(account.Id.ToString());
            model.UpdatedTime = DateTime.Now;
            model.TrueName = account.TrueName;
            model.Logo = account.Logo;
            int index = HomeBLL.sysUserBLL.Edit(ref errors, model);
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);

        }
        #endregion

        #region 修改密码
        public async Task<ActionResult> UserPwdSet()
        {
            AccountModel account = await GetAccount();
            if (account != null)
            {
                return View();
            }
            return Redirect("/");
        }
        [HttpPost]
        public async Task<JsonResult> UserPwdSet(UserPwdSetViewModel model)
        {
            AccountModel account = await GetAccount();
            if (model.NewPwd != model.AgainPWd)
            {
                return Json(JsonHandler.CreateMessage(0, "两次密码输入不一致"));
            }
            else
            {
                var user = HomeBLL.sysUserBLL.LoginUser(account.UserName, model.OldPwd);
                if (user != null)
                {
                    user.UpdatedTime = DateTime.Now;
                    user.Password = Method.GetMD5(model.NewPwd);
                    int index = HomeBLL.sysUserBLL.Edit(ref errors, user);
                    var json = JsonHandler.CreateMessage(index, errors.MsgStr);
                    return Json(json);
                }
                else
                {
                    return Json(JsonHandler.CreateMessage(0, "初始密码输入不正确"));
                }
            }
        }
        #endregion

        #region 用户设置
        public ActionResult SystemSetting()
        {
            var model = HomeBLL.GetSystemSetting(HttpContext);
            //new SystemSettingViewModel()
            return View(model);
        }
        [HttpPost]
        public JsonResult SystemSetting(SystemSettingViewModel model)
        {

            var res = HomeBLL.SetSystemSetting(HttpContext, model);
            var json = JsonHandler.CreateMessage(res);
            return Json(json);

        }
        #endregion

        #region 首页报表
        [HttpGet]
        public JsonResult GetIpCountOfTypeNames(int daycount=7)
        {
            HomeBLL homeBLL = new HomeBLL();

            var json = homeBLL.GetIpCountOfTypeNames(daycount);
            return Json(json);

        }
        [HttpGet]
        public JsonResult GetIpCountPieChart()
        {
            HomeBLL homeBLL = new HomeBLL();
            var json = homeBLL.GetIpCountPieChart();
            return Json(json);

        }
        [HttpGet]
        public JsonResult GetUserSaleCountColumnChart()
        {
            HomeBLL homeBLL = new HomeBLL();
            var json = homeBLL.GetUserSaleCountColumnChart();
            return Json(json);

        }
        #endregion

    }

}
