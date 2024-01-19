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

    public sealed class StudentBLL : DataExecuteSql<Student>
    {

        public StudentBLL() : base()
        {

        }

        #region 查询
        public IQueryable<Student> Lists
        {
            get => GetList();
        }
        public IQueryable<Student> GetList(int page, int limit, out int totalCount, string field, string order, Student model = null)
        {
            Expression<Func<Student, bool>> lamdba1 = a => a.Id > 0;

            if (!string.IsNullOrEmpty(model.Name))
            {
                lamdba1 = a => a.Name.Contains(model.Name);
            }
            var query = GetListPage<Student>(limit, page, out totalCount, order, field,whereLambda: lamdba1);
            return query;
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

        #region 导入
        public string Import(DataTable dt, string filepath)
        {
            int rightIndex = 0;
            int errorIndex = 0;
            List<string> list = new List<string>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    string name = dt.Rows[i]["姓名"].ToString().Trim();
                    int score = int.TryParse(dt.Rows[i]["点击量"].ToString().Trim(), out score) ? score : 0;
                    Student model = new Student() { Name = name, Score = score, CreatedTime = DateTime.Now.ToString() };
                    int res = Create(model);
                    if (res >0)
                    {
                        rightIndex++;
                    }
                    else
                    {
                        errorIndex++;
                    }
                }
                catch (Exception ex)
                {
                    string str = "错误编号：" + i + "|" + ex.Message;
                    Console.WriteLine(str);
                    errorIndex++;
                    list.Add(str);
                }
            }
            string strc = "正确记录：" + rightIndex + "错误记录：" + errorIndex;
            System.IO.File.Delete(filepath);
            return strc;
        }
        #endregion

        #region 导出
        public DataTable Export()
        {

            DataTable dt = new DataTable();
            dt.Columns.Add("编号");
            dt.Columns.Add("姓名");
            dt.Columns.Add("点击量");
            dt.Columns.Add("创建时间");
            foreach (var item in Lists)
            {
                dt.Rows.Add(item.Id, item.Name, item.Score, item.CreatedTime);
            }
            return dt;
        }
        #endregion
    }

}