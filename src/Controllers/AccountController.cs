using System;
using System.Threading.Tasks;

using ChuntianCms.BLL;
using ChuntianCms.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ChuntianCms.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{


    [HttpGet]
    public ActionResult Index()
    {
        return View();
    }
    [HttpPost]
    public async Task<JsonResult> Login(string username, string password)
    {
        var user = HomeBLL.sysUserBLL.LoginUser(username, password);
        if (user != null)
        {
            await AuthCookie.SetCookie(HttpContext,username, password, "空");
            return Json(JsonHandler.CreateMessage(1, "成功登陆"));
        }
        else
        {
            return Json(JsonHandler.CreateMessage(0, "用户名或密码错误"));
        }

    }

}

