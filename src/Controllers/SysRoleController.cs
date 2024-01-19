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
    public class SysRoleController : BaseController
    {

        SysRoleBLL bll = HomeBLL.sysRoleBLL;
        ErrorMsg errors = new ErrorMsg();
        string controllerName = nameof(SysRoleController);

        #region 查询
        [SupportFilter(ActionName = "Index")]
        public ActionResult Index()
        {
            ViewBag.Title = GetTitle(controllerName, nameof(Index));
            return View();
        }
        
        [HttpGet]
        [SupportFilter(ActionName = "Index")]
        public JsonResult GetList(string page, string limit, string field, string order, string name)
        {
            int totalcount = 0;
            SysRole md = new SysRole() { Name = name };
            md = string.IsNullOrEmpty(md.Name) ? null : md;
            var list = bll.GetList(page, limit, out totalcount, field, order, md);
            JsonModel model = new JsonModel() { code = 0, count = totalcount, data = list.ToArray() };
            return Json(model);
        }
        [HttpGet]
        [SupportFilter(ActionName = "Index")]
        public JsonResult GetUserByRoleId(int roleid)
        {
            var roles = bll.GetUserByRoleId(roleid);
            string str =string.Join(",", roles.Select(a => a.UserName));
            return Json(str);
        }
        #endregion

        #region 创建
        [SupportFilter(ActionName = "Create")]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [SupportFilter(ActionName = "Create")]
        public async Task<JsonResult> Create(SysRole model)
        {
            model.CreatedTime = DateTime.Now;
            model.NewId = Method.NewId;
            model.CreatePerson = (await GetAccount()).UserName;

            int index = bll.Create(ref errors, model);
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);
        }
        #endregion

        #region 编辑
        [SupportFilter(ActionName = "Edit")]
        public ActionResult Edit()
        {
            return View();
        }
        [HttpGet]
        [SupportFilter(ActionName = "Edit")]
        public JsonResult GetEdit(string id)
        {

            var entity = bll.GetById(id);
            return Json(entity);
        }
        [HttpPost]
        [SupportFilter(ActionName = "Edit")]
        public JsonResult Edit(SysRole model)
        {
            model.UpdatedTime = DateTime.Now;
            //model.CreatePerson = GetAccount().UserName;
            int index = bll.Edit(ref errors, model);
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);
        }
        #endregion

        #region 详情
        public ActionResult Detail()
        {
            return View();
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

    }
}