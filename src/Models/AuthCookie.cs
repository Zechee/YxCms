using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ChuntianCms.Models;


public class AuthCookie
{
    #region cookie模式登陆
    /// <summary>
    /// 设置用户登陆成功凭据（Cookie存储）
    /// </summary>
    /// <param name="UserName">用户名</param>
    /// <param name="PassWord">密码</param>
    /// <param name="Rights">权限</param>
    public static async Task SetCookie(HttpContext httpContext, string UserName, string PassWord, string Rights)
    {
        var authScheme = CookieAuthenticationDefaults.AuthenticationScheme;

        string UserData = UserName + "#" + PassWord + "#" + Rights;
        var claimsIdentity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, UserData)
        }, authScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        await httpContext.SignInAsync(authScheme, claimsPrincipal, new AuthenticationProperties(new Dictionary<string, string?>
        {
            {"UserData", UserData }
        }));
        //数据放入ticket//设定cookie过期时间10个小时
        //FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, UserName, DateTime.Now, DateTime.Now.AddMinutes(600), false, UserData);
        ////数据加密
        //string enyTicket = FormsAuthentication.Encrypt(ticket);
        //HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, enyTicket);
        //cookie.HttpOnly = true;
        //cookie.Secure = FormsAuthentication.RequireSSL;
        //cookie.Domain = FormsAuthentication.CookieDomain;
        //cookie.Path = FormsAuthentication.FormsCookiePath;
        //cookie.Expires = DateTime.Now.AddMinutes(600);
        ////cookie.Expires = DateTime.Now.AddMinutes(5);
        //HttpContext.Current.Response.Cookies.Remove(FormsAuthentication.FormsCookieName);
        //HttpContext.Current.Response.Cookies.Add(cookie);

        //var expiresStr = "UserExpiresTime";
        //HttpContext.Current.Response.Cookies.Remove(expiresStr);
        //HttpCookie cookie2 = new HttpCookie(expiresStr, cookie.Expires.ToString());
        //cookie2.Expires = cookie.Expires;
        //HttpContext.Current.Response.Cookies.Add(cookie2);

    }
    /// <summary>
    /// 判断用户是否登陆
    /// </summary>
    /// <returns>True,Fales</returns>
    public static bool IsLogin(HttpContext httpContext)
    {
        return httpContext.User.Identity.IsAuthenticated;
    }
    /// <summary>
    /// 注销登陆
    /// </summary>
    public static void LoginOut()
    {
        //FormsAuthentication.SignOut();
    }
    /// <summary>
    /// 获取凭据中的用户名
    /// </summary>
    /// <returns>用户名</returns>
    public static async Task<string> GetUserName(HttpContext httpContext)
    {
        if (IsLogin(httpContext))
        {
            var token = await httpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "UserData");
            var strUserData = httpContext.User.Identity.Name;
            //string strUserData = ((FormsIdentity)(HttpContext.Current.User.Identity)).Ticket.UserData;
            string[] UserData = strUserData.Split('#');
            if (UserData.Length != 0)
            {
                return UserData[0].ToString();
            }
        }
        return "";
    }
    /// <summary>
    /// 获取凭据中的密码
    /// </summary>
    /// <returns>密码</returns>
    public static async Task<string> GetPassWord(HttpContext httpContext)
    {
        if (IsLogin(httpContext))
        {
            var token = await httpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "UserData");
            var strUserData = httpContext.User.Identity.Name;
            //string strUserData = ((FormsIdentity)(HttpContext.Current.User.Identity)).Ticket.UserData;
            string[] UserData = strUserData.Split('#');
            if (UserData.Length != 0)
            {
                return UserData[1].ToString();
            }
        }
        return "";
    }
    /// <summary>
    /// 获取凭据中的用户权限
    /// </summary>
    /// <returns>用户权限</returns>
    public static async Task<string> GetRights(HttpContext httpContext)
    {
        if (IsLogin(httpContext))
        {
            var token = await httpContext.GetTokenAsync(CookieAuthenticationDefaults.AuthenticationScheme, "UserData");
            var strUserData = httpContext.User.Identity.Name;
            //string strUserData = ((FormsIdentity)(HttpContext.Current.User.Identity)).Ticket.UserData;
            string[] UserData = strUserData.Split('#');
            if (UserData.Length != 0)
            {
                return UserData[2].ToString();
            }
        }
        return "";
    }
    //public static string GetUserExpiresTime()
    //{
    //    if (HttpContext.Current.Request.Cookies["UserExpiresTime"] != null)
    //    {
    //        HttpCookie aCookie = HttpContext.Current.Request.Cookies["UserExpiresTime"];

    //        DateTime dtend = DateTime.Parse(aCookie.Value);
    //        string str = Method.ExecDateDiff(DateTime.Now, dtend);
    //        return str;//+ "后退出"
    //    }
    //    else
    //    {
    //        return "";
    //    }
    //}

    #endregion
}

