using System;
using System.Linq;
using System.Transactions;
using ChuntianCms.DAL;
using ChuntianCms.Models;
using ChuntianCms.Helper;


namespace ChuntianCms.BLL
{
    public sealed class SysMenuBLL : DataExecuteSql<SysMenu>
    {
        
        public SysMenuBLL() : base()
        {

        }

        #region 查询
        public IQueryable<SysMenu> Lists
        {
            get => GetList();
        }
        public IQueryable<SysMenu> ListMenus
        {
            get => GetList().Where(a => a.IsMenu == 1);
        }
        public IQueryable<SysMenu> GetList(string page, string limit, out int totalCount, string field, string order)
        {
            int currentPage = int.TryParse(page, out currentPage) ? currentPage : 1;
            int rows = int.TryParse(limit, out rows) ? rows : 1;
            rows = rows == 0 ? 1 : rows;
            var query = Lists.Where(a => a.IsMenu == 1);
            totalCount = query.Count();
            var queryData = LinqHelper.SortingAndPaging(query, field, order, currentPage, rows);
            return queryData;
        }
        public SysMenu GetById(string id)
        {
            return FirstOrDefault(a => a.Id.ToString() == id);
        }
        #endregion

        #region 创建
        public int Create(ref ErrorMsg errors, SysMenu model)
        {

            try
            {
                //写入的时候是否包含主键
                bool isContainKey = false;
                SysMenu entity = FirstOrDefault(a => a.Id == model.Id);
                if (entity != null)
                {

                    errors.Add("主键重复");
                    return 0;
                }
                else
                {

                    int result = Create(model, isContainKey);
                    entity = FirstOrDefault(a => a.NewId == model.NewId);
                    model.Id = entity.Id;
                    if (result > 0 && HomeBLL.sysRightBLL.AddRight(model, null) == 1)
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
       
        #endregion

        #region 编辑
        public int Edit(ref ErrorMsg errors, SysMenu model)
        {
            try
            {
                SysMenu entity = GetById(model.Id.ToString());
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
                    flag = HomeBLL.sysRightBLL.DeleteRight(idstrs,"menu");
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

    }
}