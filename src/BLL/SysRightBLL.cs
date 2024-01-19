using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using ChuntianCms.DAL;
using ChuntianCms.Models;
using ChuntianCms.Helper;

namespace ChuntianCms.BLL
{
    public sealed class SysRightBLL : DataExecuteSql<SysRight>
    {

        public SysRightBLL() : base()
        {

        }

        #region 查询
        public IQueryable<SysRight> Lists
        {
            get => GetList();
        }
        public IQueryable<SysRight> GetList(string page, string limit, out int totalCount, string field, string order)
        {
            int currentPage = int.TryParse(page, out currentPage) ? currentPage : 1;
            int rows = int.TryParse(limit, out rows) ? rows : 1;
            rows = rows == 0 ? 1 : rows;
            totalCount = Lists.Count();
            var queryData = LinqHelper.SortingAndPaging(Lists, field, order, currentPage, rows);
            return queryData;
        }
        public SysRight GetById(string id)
        {
            return FirstOrDefault(a => a.Id.ToString() == id);
        }
        #endregion

        #region 创建
        public int Create(ref ErrorMsg errors, SysRight model)
        {

            try
            {
                //写入的时候是否包含主键
                bool isContainKey = false;
                SysRight entity = FirstOrDefault(a => a.Id == model.Id);
                if (entity != null)
                {

                    errors.Add("主键重复");
                    return 0;
                }
                else
                {

                    int result = Create(model, isContainKey);
                    if (result > 0)
                    {
                        errors.Add("写入数据库成功");
                        return 1;
                    }
                    else
                    {
                        errors.Add("写入数据库失败");
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return 0;
            }
        }
        public int AddRight(SysMenu modelMenu = null, SysRole modelRole = null)
        {
            int index = 0;
            if (modelRole != null)
            {
                foreach (var item in HomeBLL.sysMenuBLL.Lists)
                {
                    SysRight entity = new SysRight();
                    entity.Id = Method.NewId;
                    entity.ModuleId = item.Id;
                    entity.RoleId = modelRole.Id;
                    entity.Rightflag = false;
                    entity.CreatedTime = DateTime.Now;
                    index += HomeBLL.sysRightBLL.Create(entity, true);

                }
            }
            if (modelMenu != null)
            {
                foreach (var item in HomeBLL.sysRoleBLL.Lists)
                {
                    SysRight entity = new SysRight();
                    entity.Id = Method.NewId;
                    entity.ModuleId = modelMenu.Id;
                    entity.RoleId = item.Id;
                    entity.Rightflag = false;
                    entity.CreatedTime = DateTime.Now;
                    index += HomeBLL.sysRightBLL.Create(entity, true);

                }
            }
            return index > 0 ? 1 : 0;
        }

        #endregion

        #region 编辑
        public int Edit(ref ErrorMsg errors, SysRight model)
        {
            try
            {
                SysRight entity = GetById(model.Id.ToString());
                if (entity == null)
                {
                    errors.Add("找不到实体");
                    return 0;
                }
                else if (Edit(model) == 1)
                {
                    errors.Add("修改成功");
                    return 1;
                }
                else
                {
                    errors.Add("修改数据库失败");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return 0;
            }
        }

        #endregion

        #region 删除
        public int DeleteRight(string idstrs, string type = "menu")
        {
            string[] idsArr = idstrs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int index = 0;
            for (int i = 0; i < idsArr.Length; i++)
            {
                string id = idsArr[i];
                int newid;
                IQueryable<SysRight> sr = null;
                if (type == "menu")
                {
                    newid = HomeBLL.sysMenuBLL.GetById(id).Id;
                    sr = HomeBLL.sysRightBLL.Lists.Where(a => a.ModuleId == newid);
                }
                else
                {
                    newid = HomeBLL.sysRoleBLL.GetById(id).Id;
                    sr = HomeBLL.sysRightBLL.Lists.Where(a => a.RoleId == newid);
                }
                foreach (var o in sr)
                {
                    index += HomeBLL.sysRightBLL.Delete(o.Id);
                }
            }
            if (index > 0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        public int Delete(ref ErrorMsg errors, string idstrs, string[] deleteCollection = null)
        {
            int flag = 0;
            try
            {

                if (deleteCollection != null)
                {
                    flag = DeleteCommon(deleteCollection);
                }
                if (deleteCollection == null)
                {
                    string[] idsArr = idstrs.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    flag = DeleteCommon(idsArr);
                }
                if (flag == 0)
                {
                    errors.Add("删除失败");
                }
                if (flag == 1)
                {
                    errors.Add("删除成功");
                }
                return flag;
            }
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return 0;
            }
        }
        private int DeleteCommon(string[] deleteCollection)
        {
            using (TransactionScope transactionScope = new TransactionScope())
            {
                if (Delete(deleteCollection) == deleteCollection.Length)
                {
                    transactionScope.Complete();
                    return 1;
                }
                else
                {
                    Transaction.Current.Rollback();
                    return 0;
                }
            }
        }
        #endregion

        #region 更新权限通过权限操作模块
        public int UpdateRightNext(ref ErrorMsg errors, SelectModel model)
        {

            string[] arrs = Method.StringSplitToArray(model.value, ",");
            SysRight right = FirstOrDefault(a => a.ModuleId.ToString() == arrs[0] && a.RoleId.ToString() == arrs[1]);
            bool rightflag = model.selected == "1";
            return UpdateRightCommon(ref errors, right, rightflag);
        }
        public int UpdateRight(ref ErrorMsg errors, SelectModel model)
        {

            SysRight right = GetById(model.value);
            bool rightflag = model.selected == "selected";
            return UpdateRightCommon(ref errors,right,rightflag);
        }
        public int UpdateRightCommon(ref ErrorMsg errors, SysRight right,bool rightflag)
        {

            
            int flag = 0;
            if (right != null)
            {

                right.Rightflag = rightflag;
                right.UpdatedTime = DateTime.Now;
                flag = HomeBLL.sysRightBLL.Edit(right);
                if (flag == 1)
                {
                    if (UpdateSysRightRightFlag(right.ModuleId, right.RoleId) == 1)
                    {
                        flag = 1;
                    }
                    else
                    {
                        flag = 0;
                        errors.Add("UpdateSysRightRightFlag更新数据库失败");
                    }
                }
                else
                {
                    flag = 0;
                    errors.Add("UpdateRight,SysRightOperate更新数据库失败");
                }
            }
            else
            {
                errors.Add("更新数据库失败");
                flag = 0;
            }
            return flag;
        }
        /// <summary>
        /// 将存储过程转换为linq查询和数据库上下文操作根据权限操作表的外键rightid查出权限表唯一记录
        /// 然后只要权限操作表有一个为真，该权限就为真，权限为真菜单就可以看见使用，
        /// 直接父菜单可以使用必然父级的父级等等都应该可以使用，但是如果权限操作表都为假即每个按钮都不可用，
        /// 那么父菜单必然不可用即权限表rightFlag标志为假，但是父级的父级菜单rightFlag也是动态变化的，所以需要
        /// 判断父级菜单的兄弟菜单权限标志是否为假，都为假，父级的父级为假，一个为真，父级的父级为真，不用更改，
        /// 只有都为假的时候不断向上递归直到顶级菜单退出循环。
        /// </summary>
        /// <param name="firstRightId"></param>
        private int UpdateSysRightRightFlag(int menuid, int roleid)
        {
            int flag = 0;//运行记录状态
            //动态调整当前权限的菜单编号
            int currentModuleId = menuid;
            //判断是不是根节点不是根节点就循环所有子节点
            while (currentModuleId != -1)
            {
                //查询该模块有没有父节点
                currentModuleId = (from sm in HomeBLL.sysMenuBLL.Lists where sm.Id == currentModuleId select sm.ParentId).SingleOrDefault();
                //说明是最顶层的节点永远都不可能不存在这个模块，currentModuleId=0退出循环
                if (currentModuleId == -1)
                {
                    flag = 1;
                }
                //查询该节点下的所有子节点的权限集合个数就是自己的兄弟节点菜单id集合
                var childids = from sm in HomeBLL.sysMenuBLL.Lists where sm.ParentId == currentModuleId select sm.Id;
                //统计兄弟权限的可用标志数量决定父级权限是否可用
                var getrightcount = (from sr in HomeBLL.sysRightBLL.Lists
                                     where childids.Contains(sr.ModuleId) &
             sr.RoleId == roleid & sr.Rightflag == true
                                     select sr.Id).Count();
                //唯一的父级权限
                var sysRightParent = (from sr in HomeBLL.sysRightBLL.Lists where sr.ModuleId == currentModuleId & sr.RoleId == roleid select sr).SingleOrDefault();
                //设定父级权限是否可用
                if (sysRightParent != null)
                {
                    sysRightParent.Rightflag = getrightcount > 0 ? true : false;
                    sysRightParent.UpdatedTime = DateTime.Now;
                    flag = HomeBLL.sysRightBLL.Edit(sysRightParent);
                }

            }
            return flag;
        }
        #endregion

        #region 查出一个角色对某个模块的所有按钮的权限列表结果集
        public IQueryable<SelectModel> GetRightOpts(int roleId, int moduleId)
        {

            //第1步根据模块id和角色id查出唯一的权限id集合
            var sysmeus = from sm in HomeBLL.sysMenuBLL.Lists where sm.ParentId == moduleId select sm;
            var result = from sr in HomeBLL.sysRightBLL.Lists
                         from sm in sysmeus
                         where sr.ModuleId == sm.Id && sr.RoleId == roleId
                         select new SelectModel
                         {
                             name = sm.Name,
                             value = sr.Id,
                             selected = sr.Rightflag == true ? "selected" : ""
                         };
            return result;
        }

        #endregion

        #region 权限管理展示模块
        public bool? GetRightFlagOpts(int roleId, int moduleId)
        {

            var result = (from sr in HomeBLL.sysRightBLL.Lists
                          where sr.ModuleId == moduleId && sr.RoleId == roleId
                          select sr.Rightflag).SingleOrDefault();
            return result;
        }
        public List<SysMenu> GetAllMenus()
        {
            var listsrc = HomeBLL.sysMenuBLL.Lists.Where(a => a.ParentId != -1);
            List<SysMenu> reslist = new List<SysMenu>();
            GetAllChilds(ref reslist, listsrc, 1);
            foreach (var item in reslist)
            {
                int count = ParentCount(item.ParentId, listsrc);
                item.Name = GetTreeValueByParentCount(count) + item.Name;
            }

            return reslist;
        }
        public List<SysRole> GetAllRoles()
        {
            return HomeBLL.sysRoleBLL.Lists.ToList();
        }
        private void GetAllChilds(ref List<SysMenu> reslist, IQueryable<SysMenu> lists, int id)
        {
            // var model = lists.Where(a => a.Id == id).SingleOrDefault();

            var list = lists.Where(a => a.ParentId == id);
            foreach (var item in list)
            {
                reslist.Add(item);
                GetAllChilds(ref reslist, lists, item.Id);
            }
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
        private int ParentCount(int parentId, IQueryable<SysMenu> lists)
        {
            List<SysMenu> list = new List<SysMenu>();
            GetAllParents(ref list, lists, parentId);
            return list.Count;
        }
        private string GetTreeValueByParentCount(int count)
        {
            string str = "";
            for (int i = 0; i < count; i++)
            {
                str += "--";
            }
            return str;
        }
        #endregion
    }

}
