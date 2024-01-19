using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChuntianCms.BLL;
using ChuntianCms.Models;

namespace ChuntianCms.Controllers
{
    public class SysUserController : BaseController
    {

        SysUserBLL bll = HomeBLL.sysUserBLL;
        ErrorMsg errors = new ErrorMsg();
        string controllerName = nameof(SysUserController);

        #region 查询
        [SupportFilter(ActionName = "Query")]
        public ActionResult Index()
        {
            ViewBag.Title = GetTitle(controllerName, nameof(Index));
            return View();
        }
        [HttpGet]
        [SupportFilter(ActionName = "Query")]
        public JsonResult GetList(string page, string limit, string field, string order, string name)
        {
            int totalcount = 0;
            SysUser md = new SysUser() { UserName = name };
            md = string.IsNullOrEmpty(md.UserName) ? null : md;
            var list = bll.GetList(page, limit, out totalcount, field, order, md);
           
            JsonModel model = new JsonModel() { code = 0, count = totalcount, data = list };
            return Json(model);
        }

        [HttpGet]
        public JsonResult GetRoles()
        {
            var roles = HomeBLL.sysRoleBLL.Lists;
            return Json(roles);
        }
        #endregion

        #region 创建
        [SupportFilter(ActionName = "Create")]
        public ActionResult Create()
        {
            ViewBag.Title = GetTitle(controllerName, nameof(Create));
            return View();
        }
        [HttpPost]
        [SupportFilter(ActionName = "Create")]
        public JsonResult Create(SysUser model)
        {
            model.CreatedTime = DateTime.Now;
            model.Password = Method.GetMD5(model.Password);
            int index = bll.Create(ref errors, model);
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);
        }
        #endregion

        #region 修改
        [HttpGet]
        [SupportFilter(ActionName = "Edit")]
        public JsonResult GetEdit(string id)
        {

            var entity = bll.GetById(id);
            return Json(entity);
        }
        [HttpPost]
        [SupportFilter(ActionName = "Edit")]
        public JsonResult Edit(SysUser model)
        {
            model.UpdatedTime = DateTime.Now;
            int index = bll.Edit(ref errors, model);
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);
        }
      
        #endregion

        #region 删除
        [HttpPost]
        [SupportFilter(ActionName = "Delete")]
        public JsonResult Delete(string id)
        {
            
            int index = bll.Delete(ref errors, id);
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);

        }
        #endregion

        #region 上传
        [HttpPost]
        public JsonResult Upload()
        {
            string dirpath = "wwwroot/upload/logo/";
            return Json(HomeBLL.SysUserUploadFile(HttpContext, dirpath));
        }
        #endregion
    }
}
