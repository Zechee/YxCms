using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using Microsoft.AspNetCore.Mvc;

using ChuntianCms.Models;
using ChuntianCms.BLL;
using ChuntianCms.Helper;


namespace ChuntianCms.Controllers
{

    public class StudentController : BaseController
    {
        private StudentBLL bll = HomeBLL.studentBLL;
        ErrorMsg errors = new ErrorMsg();
        public StudentController()
        {
           
        }

        #region 查询
        public ActionResult Index()
        {
            ViewBag.Title = "作者管理";
            return View();
        }
        
        [HttpGet]
        public JsonResult GetList(int page, int limit, string field = "Id", string order = "desc",string name = "")
        {

            int totalcount = 0;
            Student model = new Student() { Name = name };
            var list = bll.GetList(page, limit, out totalcount, field, order, model);
            JsonModel json = new JsonModel() { code = 0, count = totalcount, data = list };
            return Json(json);
        }
       
        #endregion

        #region 删除
        [HttpPost]
        public JsonResult Delete(string id)
        {
            int index = bll.Delete(ref errors, id);
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);
        }
        #endregion

        #region 导入
        [HttpGet]
        public JsonResult Import(string filename)
        {
            string path = "wwwroot/upload/file/" + filename;
            string filepath = Method.GetServerPath(HttpContext,path);
            DataTable dt = ExcelHelper.Import(filepath, false);
            var json = bll.Import(dt, filepath);
            return Json(json);
        }
        [HttpPost]
        public JsonResult UploadFile()
        {

            string dirpath = "wwwroot/upload/file/";
            var json = HomeBLL.SysUserUploadFile(HttpContext,dirpath);
            return Json(json);
        }
        #endregion


        #region 导出
        [HttpGet]
        public IActionResult Export()
        {
            string filename = "数据报表";
            DataTable dt = bll.Export();
            List<string> columnList = new List<string>();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                columnList.Add(dt.Columns[i].ColumnName);
            }
            string[] oldColumns = columnList.ToArray();
            string[] newColumns = columnList.ToArray();
            string sheetname = "数据";
            filename = filename + ".xlsx";
            var bytes = ExcelHelper.ExportByWeb(dt, "", oldColumns, newColumns, sheetname);
            //// 设置编码和附件格式      
            //Response.ContentType = "application/vnd.ms-excel";
            //context.Response.ContentEncoding = Encoding.UTF8;
            //context.Response.Charset = "";
            //context.Response.AppendHeader("Content-Disposition",
            //    "attachment;filename=" + HttpUtility.UrlEncode(strFileName, Encoding.UTF8));
            //return Json(dt);
            return new FileContentResult(bytes, "application/vnd.ms-excel");

        }
        #endregion


    }
}