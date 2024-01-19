using System;
using System.Linq;
using System.Transactions;
using ChuntianCms.DAL;
using ChuntianCms.Models;
using ChuntianCms.Helper;

namespace ChuntianCms.BLL
{
    public sealed class SysRoleBLL : DataExecuteSql<SysRole>
    {
        
        public SysRoleBLL() : base()
        {

        }

        #region 查询
        public IQueryable<SysRole> Lists
        {
            get => GetList();
        }
        public IQueryable<SysRole> GetList(string page, string limit, out int totalCount, string field, string order, SysRole model = null)
        {
            int currentPage = int.TryParse(page, out currentPage) ? currentPage : 1;
            int rows = int.TryParse(limit, out rows) ? rows : 1;
            rows = rows == 0 ? 1 : rows;
            var query = Lists;
            if (model != null)
            {
                query = query.Where(a => a.Name == model.Name);
            }
            totalCount = query.Count();
            var queryData = LinqHelper.SortingAndPaging(query, field, order, currentPage, rows);
            return queryData;
        }
        public SysRole GetById(string id)
        {
            return FirstOrDefault(a => a.Id.ToString() == id);
        }
      
        #endregion

        #region 创建
        public int Create(ref ErrorMsg errors, SysRole model)
        {

            try
            {
                //写入的时候是否包含主键
                bool isContainKey = false;
                SysRole entity = FirstOrDefault(a => a.Id == model.Id);
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
                    if (result > 0 &&HomeBLL.sysRightBLL.AddRight(null,model) == 1)
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
        public int Edit(ref ErrorMsg errors, SysRole model)
        {
            try
            {
                SysRole entity = GetById(model.Id.ToString());
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
                    flag = HomeBLL.sysRightBLL.DeleteRight(idstrs, "role");
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

        #region 其它查询
        public IQueryable<SysUser> GetUserByRoleId(int roleId)
        {

            IQueryable<SysUser> queryData = from a in HomeBLL.sysUserBLL.Lists
                                            where a.RoleId == roleId select a;
                                                
            return queryData;
        }
        #endregion

    }
}