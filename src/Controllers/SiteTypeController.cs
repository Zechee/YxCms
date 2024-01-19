using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChuntianCms.Models;
using ChuntianCms.BLL;


namespace ChuntianCms.Controllers
{

    public class SiteTypeController : BaseController
    {
        private SiteTypeBLL bll = HomeBLL.siteTypeBLL;
        ErrorMsg errors = new ErrorMsg();
        public SiteTypeController()
        {
           
        }

        #region 查询
        [SupportFilter(ActionName = "Query")]
        public ActionResult Index()
        {

            ViewBag.Title = "类别管理";
            return View();
        }
        
        [HttpGet]
        [SupportFilter(ActionName = "Query")]
        public JsonResult GetList(int page, int limit, string field = "Id", string order = "desc",string name = "")
        {

            int totalcount = 0;
            var list = bll.GetList(page, limit, out totalcount, field, order);
            JsonModel json = new JsonModel() { code = 0, count = totalcount, data = list };
            return Json(json);
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
        public JsonResult Create(SiteType model)
        {
            model.CreatedTime = DateTime.Now.ToString();
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
        public JsonResult Edit(SiteType model)
        {
            model.UpdatedTime = DateTime.Now.ToString();
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


    }
}