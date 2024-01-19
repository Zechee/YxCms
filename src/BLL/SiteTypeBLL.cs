using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using ChuntianCms.Models;
using ChuntianCms.DAL;
using System.Transactions;

namespace ChuntianCms.BLL
{

    public sealed class SiteTypeBLL : DataExecuteSql<SiteType>
    {

        public SiteTypeBLL() : base()
        {

        }

        #region 查询
        public IQueryable<SiteType> Lists
        {
            get => GetList();
        }
        public IQueryable<SiteType> GetList(int page, int limit, out int totalCount, string field, string order, SiteType model = null)
        {
           
            var query = GetListPage<SiteType>(limit, page, out totalCount, order, field);
            return query;
        }
      
        public SiteType GetById(string id)
        {
            return FirstOrDefault(a => a.Id.ToString() == id);
        }
        public SiteType GetByName(string name)
        {
            return FirstOrDefault(a => a.Name == name);
        }
        #endregion

        #region 创建
        public int Create(ref ErrorMsg errors, SiteType model)
        {

            try
            {
                //写入的时候是否包含主键
                bool isContainKey = false;
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
            catch (Exception ex)
            {
                errors.Add(ex.Message);
                return 0;
            }
        }
        #endregion

        #region 编辑
        public int Edit(ref ErrorMsg errors, SiteType model)
        {
            try
            {
                SiteType entity = GetById(model.Id.ToString());
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