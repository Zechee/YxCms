using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using ChuntianCms.Models;
using ChuntianCms.BLL;

namespace ChuntianCms.Controllers
{
    public class SysRightController : BaseController
    {

        public SysRightBLL sysRightBLL = HomeBLL.sysRightBLL;
        ErrorMsg errors = new ErrorMsg();
        string controllerName = nameof(SysRoleController);

        #region 1.查询
        [SupportFilter(ActionName = "Index")]
        public ActionResult Index()
        {
            ViewBag.Title = GetTitle(controllerName, nameof(Index));

            return View();
        }
       
        [HttpGet]
        [SupportFilter(ActionName = "Index")]
        public JsonResult GetRightOpts(int roleid, int menuid)
        {
            var json = sysRightBLL.GetRightOpts(roleid, menuid);
            return Json(json);
        }
        #endregion
      
        #region 2.修改
        [HttpPost]
        [SupportFilter(ActionName = "Index")]
        public JsonResult UpdateRight(SelectModels models)
        {
            int index = 0;
            for (int i = 0; i < models.data.Length; i++)
            {
                var model = models.data[i];
                index += sysRightBLL.UpdateRight(ref errors, model);
            }
            index = index > 0 ? 1 : 0;
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);
           
        }
        #endregion

        #region 展示并更新

        [HttpPost]
        public JsonResult UpdateRightNext(SelectModel model)
        {

            var index = sysRightBLL.UpdateRightNext(ref errors, model);
            var json = JsonHandler.CreateMessage(index, errors.MsgStr);
            return Json(json);

        }
        #endregion
    }
}