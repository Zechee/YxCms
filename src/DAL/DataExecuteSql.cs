using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Dapper;

namespace ChuntianCms.DAL
{
    //虚拟类把增删改查常用的方法抽象出来
    public abstract class DataExecuteSql<TModel> where TModel:class
    {
        private DataBase dbcontext;
     
        public DataExecuteSql()
        {
            dbcontext = new DataBase();
            DataMapping<TModel>.DbContext = dbcontext;
        }

        #region Select * 查询+Query()带查询条件的Select查询+Query(Func<TModel, bool> selector)
        /// <summary>
        /// Select * 查询arrs是字段集合，type=0查全部字段，type=1查需要的字段,type=2查过滤后的字段
        /// </summary>    
        /// <returns></returns>
        public virtual IQueryable<TModel> GetList(string[] arrs = null, int type = 0, Expression<Func<TModel, bool>> whereLambda = null, object[] IdList = null)
        {
            string sql = DataMapping<TModel>.GetQuerySql();
            switch (type)
            {
                case 0:
                    break;
                case 1:
                    sql = DataMapping<TModel>.GetQuerySqlFileds(arrs);
                    break;
                case 2:
                    sql = DataMapping<TModel>.GetQuerySqlFilterFileds(arrs);
                    break;

            }
            var primaryKey = DataMapping<TModel>.PrimaryKey;
            sql = IdList == null ? sql : sql + " where  " + primaryKey + " in @IdList";
            var parm = IdList == null ? null : new { IdList };
            return dbcontext.Query(sql, parm, whereLambda);
        }
        /// <summary>
        /// 带分页查询和lambda表达式的where条件得到的结果集
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="total"></param>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public virtual IQueryable<TModel> GetListPage<S>(int pageSize, int pageIndex, out int total
           , string order = "", string field = "", string[] arrsfield = null, int type = 0, Expression<Func<TModel, bool>> whereLambda = null)
        {
            var queryable = GetList(arrsfield, type, whereLambda);
            total = queryable.Count();
            bool isAsc = order.ToLower() == "asc";
            queryable = queryable.Ex_OrderBy(field, isAsc).Skip(pageSize * (pageIndex - 1)).Take(pageSize);
            return queryable;
        }
       
        #endregion

        #region  得到一个对象的实例+FirstOrDefault(Func<TModel, bool> selector = null)
        /// <summary>
        /// 得到一个对象的实例
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        public virtual TModel FirstOrDefault(Func<TModel, bool> selector = null)
        {
            string sql = DataMapping<TModel>.GetQuerySql();
            return dbcontext.FirstOrDefault(sql, selector);
        }
        public virtual TModel LastOrDefault(Func<TModel, bool> selector = null)
        {
            string sql = DataMapping<TModel>.GetQuerySql();
            return dbcontext.LastOrDefault(sql, selector);
        }
        public virtual TModel SingleOrDefault(Func<TModel, bool> selector = null)
        {
            string sql = DataMapping<TModel>.GetQuerySql();
            return dbcontext.SingleOrDefault(sql, selector);
        }
        public virtual IEnumerable<TModel> Query()
        {
            string sql = DataMapping<TModel>.GetQuerySql();
            return dbcontext.Query<TModel>(sql, null);
        }
        #endregion

        #region Insert非Null属性的对象实例+Insert(TModel model)
        /// <summary>
        /// Insert非Null属性的对象实例
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual int Create(TModel model, bool isContainKey = false)
        {
            string sql = DataMapping<TModel>.GetInsertSql(model, isContainKey);
            Console.WriteLine(sql);
            return dbcontext.Execute(sql, model);
        }
        #endregion

        #region Update 一个非Null属性的对象+Update(TModel model)
        /// <summary>
        /// Update 一个非Null属性的对象
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual int Edit(TModel model)
        {
            return dbcontext.Execute(DataMapping<TModel>.GetUpdateSql(model), model);
        }
        #endregion

        #region 批量删除+Delete(string[] IdList)
        public virtual int Delete(object Id)
        {
            return dbcontext.Execute(DataMapping<TModel>.GetDeleteSqlOne(),new { Id });
        }
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="IdList"></param>
        /// <returns></returns>
        public virtual int Delete(params object[] IdList)
        {
            return dbcontext.Execute(DataMapping<TModel>.GetDeleteSql(), new { IdList });
        }
        #endregion

        #region 获取多个数据集+MultyQuery(string sql, object param = null)
        /// <summary>
        /// 获取多个数据集
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual SqlMapper.GridReader MultyQuery(string sql, object param = null)
        {
            return dbcontext.MultyQuery(sql, param);
        }
        #endregion

    }
}
