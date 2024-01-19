using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChuntianCms.Models;
using ChuntianCms.BLL;
using Newtonsoft.Json;

namespace ChuntianCms.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {

        }
        /// <summary>
        /// 获取页面标题
        /// </summary>
        /// <param name="controller">控制器</param>
        /// <param name="action">活动</param>
        /// <returns>结果</returns>
        public string GetTitle(string controller = "首页", string action = "开始")
        {
            Dictionary<string, string> controllers = new Dictionary<string, string>();
            if (controller.IndexOf("Controller") != -1)
            {
                controller = controller.Substring(0, controller.IndexOf("Controller"));
            }
            controllers.Add("SysUser", "用户管理");
            controllers.Add("SysRole", "角色管理");
            controllers.Add("SysMenu", "菜单管理");
            controllers.Add("SysRight", "权限管理");

            Dictionary<string, string> actions = new Dictionary<string, string>();
            actions.Add("Index", "开始");
            actions.Add("Create", "创建");
            actions.Add("Edit", "编辑");
            actions.Add("Detail", "详情");
            if (controllers.ContainsKey(controller))
            {
                controller = controllers[controller];
            }
            if (actions.ContainsKey(action))
            {
                action = actions[action];
            }
            return controller + "_" + action;
        }
        /// <summary>
        /// 获取当前页或操作访问权限
        /// </summary>
        /// <returns>权限列表</returns>
        public List<PermModel> GetPermission()
        {
            var filePath = HttpContext.Request.Path;
            //List<PermModel> perm = (List<PermModel>)Session[filePath];
            var json = HttpContext.Session.GetString(filePath);
            List<PermModel> perm = !string.IsNullOrEmpty(json) ? JsonConvert.DeserializeObject<List<PermModel>>(json!) : null;
            return perm;
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <returns>用户信息</returns>
        public async Task<AccountModel> GetAccount()
        {
            return await HomeBLL.GetCurrentUser(HttpContext);
        }
    }
}