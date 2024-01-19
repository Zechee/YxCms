using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;

using ChuntianCms.BLL;

namespace ChuntianCms.Models;

//我们创建一个筛选器在App.Admin下的Core创建SupportFilter.cs
//Index无需填写操作码将自动创建操作码，如果你拥有一个操作码那么index将被授权,这个是我们与系统之间的一个约定(你可以去掉这个约定，修改代码即可)
//假如你拥有增删改权限却没有访问列表的权限，那不是...
//OnActionExecuting负责分解，交给ValiddatePermission去生成权限
//如果写在Areas区域的也是兼容的，已经做了处理。
//如果你越权操作那么将执行 HttpContext.Current.Response.Write("你没有操作权限，请联系管理员！");
//目前位置我们已经跑通了整个系统了，接下来就是自动化的用户角色之间的授权和模块的制作了，能跑通，其他都是很简单了，对吧
//这一章比较复杂，需要对AOP编程，MVC的筛选器，和路由进行了解，才能读的比较顺。
//如果你没有读懂，那么代码敲一遍,那么你也就差不多知道了
//代码进行了大量的注释，还不懂那么留言。
//目前为止,我们一个基于按钮级别的权限系统已经全部跑通,现在，可以创建一些没有权限的Action来验证了
//我创建：（很明显我们数据库没有这个test的 action的权限），所以你别想越权操作了

#region 筛选器设定权限
public class SupportFilterAttribute : ActionFilterAttribute
{
    public string ActionName { get; set; }
    private string Area;
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        base.OnActionExecuted(filterContext);

    }
    /// <summary>
    /// Action加上[SupportFilter]在执行actin之前执行以下代码，通过[SupportFilter(ActionName="Index")]指定参数
    /// </summary>
    /// <param name="filterContext">页面传过来的上下文</param>
    public override async void OnActionExecuting(ActionExecutingContext filterContext)
    {
        //读取请求上下文中的Controller,Action,Id
        var routes = new RouteCollection();
        // RouteConfig.RegisterRoutes(routes);
        //RouteData routeData = routes.GetRouteData(filterContext.HttpContext);
        RouteData routeData = filterContext.HttpContext.GetRouteData();
        //取出区域的控制器Action,id
        string ctlName = filterContext.Controller.ToString();
        string[] routeInfo = ctlName.Split('.');
        string controller = null;
        string action = null;
        string id = null;

        int iAreas = Array.IndexOf(routeInfo, "Areas");
        if (iAreas > 0)
        {
            //取区域及控制器
            Area = routeInfo[iAreas + 1];
        }
        int ctlIndex = Array.IndexOf(routeInfo, "Controllers");
        ctlIndex++;
        controller = routeInfo[ctlIndex].Replace("Controller", "").ToLower();

        //var request = filterContext.HttpContext.Request;
        //string url = $"{request.Scheme}://{request.Host}/";
        //string[] urlArray = url.Split('/');
        var urlArray = filterContext.HttpContext.Request.Path.ToString().Split('/');
        int urlCtlIndex = Array.IndexOf(urlArray, controller);
        urlCtlIndex++;
        if (urlArray.Count() > urlCtlIndex)
        {
            action = urlArray[urlCtlIndex];
        }
        urlCtlIndex++;
        if (urlArray.Count() > urlCtlIndex)
        {
            id = urlArray[urlCtlIndex];
        }
        //url
        action = string.IsNullOrEmpty(action) ? "Index" : action;
        int actionIndex = action.IndexOf("?", 0);
        if (actionIndex > 1)
        {
            action = action.Substring(0, actionIndex);
        }
        id = string.IsNullOrEmpty(id) ? "" : id;
        //URL路径
        string filePath = filterContext.HttpContext.Request.Path;
        var account = await HomeBLL.GetCurrentUser(filterContext.HttpContext);
        if (ValiddatePermission(filterContext.HttpContext, account, controller, action, filePath))
        {
            return;
        }
        else
        {
            filterContext.Result = new EmptyResult();
            return;
        }
    }
    //有效的许可
    public bool ValiddatePermission(HttpContext context, AccountModel account, string controller, string action, string filePath)
    {
        string strHtml = "<h1>你没有操作权限，请联系管理员！</h1>";
        bool bResult = false;
        string actionName = string.IsNullOrEmpty(ActionName) ? action : ActionName;
        controller = !string.IsNullOrEmpty(Area) ? Area + "/" + controller : controller;
        if (account != null)
        {
            var json = context.Session.GetString(filePath);
            List<PermModel> perm = !string.IsNullOrEmpty(json) ? JsonConvert.DeserializeObject<List<PermModel>>(json!) : null;
            if (perm == null)
            {
                perm = HomeBLL.GetPermission(account.RoleId, controller);
                var txt = JsonConvert.SerializeObject(perm);
                context.Session.SetString(filePath, txt);
            }
            //当用户访问index时，只要权限>0就可以访问
            if (actionName.ToLower() == "index")
            {
                if (perm.Count > 0)
                {
                    bResult = true;
                }
                else
                {
                    bResult = false;
                    //    context.Response.Write(strHtml);
                }
            }
            if (actionName.ToLower() != "index")
            {
                //查询当前Action 是否有操作权限，大于0表示有，否则没有
                int count = perm.Where(a => a.KeyCode.ToLower() == actionName.ToLower()).Count();
                if (count > 0)
                {
                    bResult = true;
                }
                else
                {
                    bResult = false;
                    //    context.Response.Write(strHtml);
                }
            }
        }
        else//如果为空跳到登录页
        {
            context.Response.Redirect("/Account/Index?ReturnUrl=" + controller + "/" + actionName);
        }
        return bResult;
    }
    public override void OnResultExecuted(ResultExecutedContext filterContext)
    {
        base.OnResultExecuted(filterContext);
    }
    public override void OnResultExecuting(ResultExecutingContext filterContext)
    {
        base.OnResultExecuting(filterContext);
    }
}
#endregion
