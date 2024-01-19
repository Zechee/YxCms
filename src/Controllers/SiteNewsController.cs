using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChuntianCms.Models;
using ChuntianCms.BLL;
using System.IO;

namespace ChuntianCms.Controllers
{

    public class SiteNewsController : BaseController
    {
        private SiteNewsBLL bll = HomeBLL.siteNewsBLL;
        ErrorMsg errors = new ErrorMsg();
        public SiteNewsController()
        {

        }

        #region 查询
        [SupportFilter(ActionName = "Index")]
        public ActionResult Index()
        {

            ViewBag.Title = "文章管理";
            return View();
        }
        [HttpGet]
        [SupportFilter(ActionName = "Index")]
        public JsonResult GetTypeNames()
        {
            SiteTypeBLL stb = HomeBLL.siteTypeBLL;
            string[] arr1 = { "所有" };
            string[] arr2 = stb.GetList().Select(a => a.Name).ToArray();
            var list = Method.GetddlListByArray(arr1.Concat(arr2));
            return Json(list);
        }
        

        [HttpGet]
        [SupportFilter(ActionName = "Index")]
        public JsonResult GetList(int page, int limit, string field = "Id", string order = "desc", string title = "", string typename = "所有")
        {

            SiteNews model = new SiteNews();
            model.Title = title;
            model.TypeName = typename;
            int totalcount = 0;
            var list = bll.GetList(page, limit, out totalcount, field, order, model);
            JsonModel json = new JsonModel() { code = 0, count = totalcount, data = list };
            return Json(json);
        }

        #endregion

        #region 创建
        [HttpGet]
        [SupportFilter(ActionName = "Create")]
        public JsonResult GetTypeNames2()
        {
            SiteTypeBLL stb = HomeBLL.siteTypeBLL;
            string[] arr2 = stb.GetList().Select(a => a.Name).ToArray();
            var list = Method.GetddlListByArray(arr2);
            return Json(list);
        }
        [SupportFilter(ActionName = "Create")]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        //[ValidateInput(false)]//包含富文本需要此特性
        [SupportFilter(ActionName = "Create")]
        public JsonResult Create(SiteNews model)
        {

            model.CreatedTime = DateTime.Now.ToString();
            model.TypeId = HomeBLL.siteTypeBLL.GetByName(model.TypeName).Id;
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
        //[ValidateInput(false)]
        [SupportFilter(ActionName = "Edit")]
        public JsonResult Edit(SiteNews model)
        {
            model.UpdatedTime = DateTime.Now.ToString();
            model.TypeId = HomeBLL.siteTypeBLL.GetByName(model.TypeName).Id;
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


        #region 详情
      
        public ActionResult Detail(string id)
        {
            var entity = bll.GetById(id);
            return View(entity);
        }

        #endregion

        #region 文件上传
        [HttpPost]
        [SupportFilter(ActionName = "Edit")]
        public JsonResult UploadEditor()
        {
            //接收文件file
            //注意：不要加if(!IsPostBack)判断！
            //file1这个是上面设置FormData的参数名，必须一致！其中Request.Files[]是文件相关的在里面
            //Request["id"]是普通类型参数在里面，例如：int string ...
            var files = HttpContext.Request.Form.Files;
            //var filename = file.FileName;//原始文件名
            int flag = 0;
            string msg = "";
            List<string> list = new List<string>();
            if (files != null)
            {
                for (int i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var imgext = Path.GetExtension(file.FileName);//获取扩展名
                    var newid = Method.NewId;
                    string filename = "wwwroot/upload/news/" + newid + imgext;
                    string filepath = Method.GetServerPath(HttpContext,filename);
                    //file.SaveAs(filepath);
                    using var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write);
                    file.CopyTo(fs);
                    list.Add(filename);
                }
            }
            else
            {
                flag = 0;
                msg = "上传文件不能为空";
            }
            return Json(list);
        }
        #endregion
    }
}