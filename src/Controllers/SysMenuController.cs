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
    public class SysMenuController : BaseController
    {

        SysMenuBLL bll = HomeBLL.sysMenuBLL;
        ErrorMsg errors = new ErrorMsg();


        #region 增删改查
        [SupportFilter(ActionName = "Index")]
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [SupportFilter(ActionName = "Index")]
        public JsonResult GetList(string page, string limit, string field, string order)
        {
            int totalcount = 0;
            var list = bll.GetList(page, limit, out totalcount, "Sort", "asc");
            JsonModel model = new JsonModel() { code = 0, count = totalcount, data = list.ToArray() };
            return Json(model);
        }
        [HttpGet]
        [SupportFilter(ActionName = "Index")]
        public JsonResult GetAllList()
        {
            var list = bll.Lists.OrderBy(a=>a.Sort);
            int totalcount = list.Count();
            JsonModel model = new JsonModel() { code = 0, count = totalcount, msg = "", data = list };
            return Json(model);
        }
        [SupportFilter(ActionName = "Create")]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [SupportFilter(ActionName = "Create")]
        public JsonResult Create(SysMenu model)
        {
            model.CreateTime = DateTime.Now;
            model.NewId = Method.NewId;

            model.MenuIcon = "fa " + model.MenuIcon;
            List<SysMenu> list = new List<SysMenu>();
            GetAllParents(ref list, bll.ListMenus, model.ParentId);
            var strs = list.Select(a => a.Id + a.Name).Reverse();
            model.Speak = string.Join("/", strs) + "/" + model.Name;
            int index = bll.Create(ref errors, model);
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);
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
        public JsonResult Edit(SysMenu model)
        {
            model.UpdateTime = DateTime.Now;
            model.MenuIcon = "fa " + model.MenuIcon;
            model.ParentId = model.Id == 1 ? -1 : model.ParentId;
            List<SysMenu> list = new List<SysMenu>();
            GetAllParents(ref list, bll.ListMenus, model.ParentId);
            var strs = list.Select(a => a.Id + a.Name).Reverse();
            model.Speak = string.Join("/", strs) + "/" + model.Name;
            int index = bll.Edit(ref errors, model);
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);
        }
        public ActionResult Detail()
        {
            return View();
        }
        [HttpPost]
        [SupportFilter(ActionName = "Delete")]
        public JsonResult Delete(int id)
        {
            int index = 0;
            List<int> allchildid = new List<int>();
            GetAllChilds(ref allchildid, bll.Lists, id);
            allchildid.ForEach((sid) => { index = bll.Delete(ref errors, sid.ToString()); });
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);

        }
        #endregion

        #region 树形菜单

        #region 创建页绑定树形菜单
        [HttpGet]
        public JsonResult BindParentid()
        {
            var list = bll.ListMenus;
            List<dynamic> arrs = new List<dynamic>();
            GetAllChildsCreate(list, -1, ref arrs);
            return Json(arrs);
        }
        private void GetAllChildsCreate(IQueryable<SysMenu> srclist, int rootid, ref List<dynamic> arrs)
        {

            var list = srclist.Where(a => a.ParentId == rootid);
            foreach (var item in list)
            {
                int id = item.Id;
                string name = item.Name;
                int count = ParentCount(item.ParentId);
                name = GetTreeValueByParentCount(count) + name;
                arrs.Add(new { Id = id, Name = name });
                GetAllChildsCreate(srclist, id, ref arrs);
            }

        }
        #endregion

        #region 编辑页绑定树形菜单
        [HttpGet]
        public JsonResult BindParentidEdit(int pid)
        {
            var list = bll.ListMenus;
            List<int> filterIds = new List<int>();
            GetAllChilds(ref filterIds, list, pid);
            List<dynamic> arrs = new List<dynamic>();
            GetAllChildsEdit(list, filterIds, -1, ref arrs);

            return Json(arrs);
        }
        private void GetAllChildsEdit(IQueryable<SysMenu> srclist, List<int> filterIds, int rootid, ref List<dynamic> arrs)
        {

            var list = srclist.Where(a => a.ParentId == rootid);
            foreach (var item in list)
            {
                int id = item.Id;
                string name = item.Name;
                int index = filterIds.FindIndex(a => a == id);
                if (index == -1)
                {
                    int count = ParentCount(item.ParentId);
                    name = GetTreeValueByParentCount(count) + name;
                    arrs.Add(new { Id = id, Name = name });
                }

                GetAllChildsEdit(srclist, filterIds, id, ref arrs);
            }
        }
        private void GetAllChilds(ref List<int> allchildid, IQueryable<SysMenu> lists, int id)
        {
            allchildid.Add(id);
            var list = lists.Where(a => a.ParentId == id);
            foreach (var item in list)
            {
                GetAllChilds(ref allchildid, lists, item.Id);
            }
        }
        #endregion

        #region 公共方法
        private int ParentCount(int parentId)
        {
            List<SysMenu> list = new List<SysMenu>();
            GetAllParents(ref list, bll.ListMenus, parentId);
            return list.Count;
        }
        private void GetAllParents(ref List<SysMenu> list, IQueryable<SysMenu> lists, int parentId)
        {
            SysMenu model = lists.Where(a => a.Id == parentId).SingleOrDefault();
            if (model != null)
            {
                list.Add(model);
                GetAllParents(ref list, lists, model.ParentId);
            }
        }
        private string GetTreeValueByParentCount(int count)
        {
            string str = "";
            for (int i = 0; i < count; i++)
            {
                str += "----";
            }
            return str;
        }
        #endregion

        #endregion

        #region 绑定操作码

        [HttpGet]
        public JsonResult GetActionCodes()
        {
            List<dynamic> list = new List<dynamic>();
            list.Add(new { Id = "Create", Name = "创建" });
            list.Add(new { Id = "Edit", Name = "编辑" });
            list.Add(new { Id = "Query", Name = "查询" });
            list.Add(new { Id = "Delete", Name = "删除" });
            list.Add(new { Id = "Details", Name = "详情" });
            list.Add(new { Id = "Import", Name = "导入" });
            list.Add(new { Id = "Export", Name = "导出" });
            return Json(list);
        }
      
        #endregion

    }
}