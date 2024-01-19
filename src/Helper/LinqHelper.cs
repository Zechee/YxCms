using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ChuntianCms.Helper;
    public class LinqHelper
    {
        #region 排序并分页
        /// <summary>
        /// 排序并分页 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static IQueryable<T> SortingAndPaging<T>(IQueryable<T> source, string sortExpression, string sortDirection, int pageNumber, int pageSize)
        {
            IQueryable<T> query = DataSorting<T>(source, sortExpression, sortDirection);
            return DataPaging(query, pageNumber, pageSize);
        }
        /// <summary>
        /// 排序
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        private static IQueryable<T> DataSorting<T>(IQueryable<T> source, string sortExpression, string sortDirection)
        {
            //错误查询
            if (string.IsNullOrEmpty(sortExpression) || string.IsNullOrEmpty(sortDirection))
            {
                return source;
            }
            string sortingDir = string.Empty;
            if (sortDirection.ToUpper().Trim() == "ASC")
                sortingDir = "OrderBy";
            else if (sortDirection.ToUpper().Trim() == "DESC")
                sortingDir = "OrderByDescending";
            ParameterExpression param = Expression.Parameter(typeof(T), sortExpression);
            PropertyInfo pi = typeof(T).GetProperty(sortExpression);
            Type[] types = new Type[2];
            types[0] = typeof(T);
            types[1] = pi.PropertyType;
            Expression expr = Expression.Call(typeof(Queryable), sortingDir, types, source.Expression, Expression.Lambda(Expression.Property(param, sortExpression), param));
            IQueryable<T> query = source.AsQueryable().Provider.CreateQuery<T>(expr);
            return query;
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        private static IQueryable<T> DataPaging<T>(IQueryable<T> source, int pageNumber, int pageSize)
        {
            if (pageNumber <= 1)
            {
                return source.Take(pageSize);
            }
            else
            {
                return source.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
        }
        #endregion

        #region where条件查询

        /*
         * if (!string.IsNullOrWhiteSpace(pager.filterRules))
         *            {
         *                IEnumerable<DataFilterModel> dataFilterList = JsonHelper.DeserializeJsonToObject<List<DataFilterModel>>(pager.filterRules).Where(f => !string.IsNullOrWhiteSpace(f.value));
         *               queryData = LinqHelper.DataFilter<SysSample>(queryData.AsQueryable(), dataFilterList);
         *           }
         */
        ///<summary>
        ///通用数据列表按过滤方法
        ///</summary>
        ///<typeparam name="T">过滤的数据类型</typeparam>
        ///<param name="source">过滤的数据源</param>
        ///<paramname="dataFilterList">过滤条件集合(包含,字段名,值,操作符) </param>
        ///<returns></returns>
        public static IQueryable<T> DataFilter<T>(IQueryable<T> source, IEnumerable<DataFilterModel> datas)
        {
            T obj = Activator.CreateInstance<T>();
            PropertyInfo[] properties = obj.GetType().GetProperties();
            foreach (var item in datas)
            {
                PropertyInfo p = properties.Where(pro => pro.Name == item.field).FirstOrDefault();
                //不进行无效过滤
                if (p == null || item.value == null)
                {
                    continue;
                }
                if (p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?))
                {
                    //时间过1滤 
                    source = DateDataFilter(source, item, p);
                }
                else
                {
                    //普通过滤
                    source = OrdinaryDataFilter(source, item, p);
                }
            }
            return source;
        }
        ///<summary>
        ///普通数据过滤
        ///</summary>
        ///<typeparam name="T"></typeparam>
        ///<param name="source"></param>
        ///<param name="item"></param>
        ///<param name="p"></param> 
        ///<retums></retums>
        private static IQueryable<T> OrdinaryDataFilter<T>(IQueryable<T> source, DataFilterModel item, PropertyInfo p)
        {
            //var selectvalue = Convert.
            //          ChangeType(item.value, p.PropertyType);
            var option = (DataFliterOperatorTypeEnum)
                     Enum.Parse(typeof(DataFliterOperatorTypeEnum), item.op);
            switch (option)
            {
                case DataFliterOperatorTypeEnum.contains:
                    {
                        /* 包含, 目前只支持字符串 */
                        source = ExpressionOperate(StringContains, source, p, item.value);
                        break;
                    }
                case DataFliterOperatorTypeEnum.equal:
                    {
                        /* 等于 */
                        source = ExpressionOperate(Expression.Equal, source, p, item.value);
                        break;
                    }

                case DataFliterOperatorTypeEnum.greater:
                    {
                        /* 大于 */
                        source = ExpressionOperate(Expression.GreaterThan, source, p, item.value);
                        break;
                    }

                case DataFliterOperatorTypeEnum.greaterorequal:
                    {
                        /* 大于等于 */
                        source =
                            ExpressionOperate(Expression.GreaterThanOrEqual, source, p, item.value);
                        break;
                    }
                case DataFliterOperatorTypeEnum.less:
                    {
                        /* 小于 */
                        source = ExpressionOperate(Expression.LessThan, source, p, item.value);
                        break;
                    }
                case DataFliterOperatorTypeEnum.lessorequal:
                    {
                        /* 小于等于 */
                        source = ExpressionOperate(Expression.LessThanOrEqual, source, p, item.value);
                        break;
                    }
                default: break;
            }
            return (source);
        }
        ///<summary> 
        ///时间过滤
        ///</summary>
        ///<typeparam name="T"></typeparam>
        ///<param name="source"></param>
        ///<param name="item"></param>
        ///<param name="p"></param> 
        ///<returns></returns>
        private static IQueryable<T> DateDataFilter<T>(IQueryable<T> source, DataFilterModel item, PropertyInfo p)
        {
            var selectDate = Convert.ToDateTime(item.value);
            var option = (DataFliterOperatorTypeEnum)
            Enum.Parse(typeof(DataFliterOperatorTypeEnum), item.op);
            switch (option)
            {
                case DataFliterOperatorTypeEnum.equal:
                    {
                        //大于0时
                        source = ExpressionOperate(Expression.GreaterThanOrEqual, source, p, selectDate);
                        //小于后一天
                        var nextDate = selectDate.AddDays(1);
                        source = ExpressionOperate(Expression.LessThan, source, p, nextDate);
                        break;
                    }

                case DataFliterOperatorTypeEnum.greater:
                    {
                        //大于等于后一天
                        selectDate = selectDate.AddDays(1);
                        source = ExpressionOperate(Expression.GreaterThanOrEqual, source, p, selectDate);
                        break;
                    }

                case DataFliterOperatorTypeEnum.greaterorequal:
                    {
                        //大于等于当天
                        source = ExpressionOperate(Expression.GreaterThanOrEqual, source, p, selectDate);
                        break;
                    }

                case DataFliterOperatorTypeEnum.less:
                    {
                        //小于当天
                        source = ExpressionOperate(Expression.LessThan, source, p, selectDate);
                        break;
                    }
                case DataFliterOperatorTypeEnum.lessorequal:
                    {
                        //小于第二天
                        selectDate = selectDate.AddDays(1);
                        source = ExpressionOperate(Expression.LessThan, source, p, selectDate);
                        break;
                    }
                default: break;
            }
            return source;
        } 
        #endregion

        #region 表达式操作
        ///<summary>
        ///表达式操作
        ///</summary>
        ///<param name="right"></param>
        ///<param name="left"></param>
        ///<returns></returns>
        public delegate Expression ExpressionOpretaDelegate(Expression left, Expression right);
        ///<summary>
        ///过滤操作
        ///</summary>
        ///<typeparam name="T"></typeparam>
        //<typeparam name="V"></typeparam>
        ///<paramname="operateExpression"></ param>
        ///<param name="source"></param>
        ///<param name="p"></param> ///<param name="value"></param>
        ///<returns></returns>
        private static IQueryable<T> ExpressionOperate<T, V>(ExpressionOpretaDelegate operateExpression, IQueryable<T> source, PropertyInfo p, V value)
        {


            Expression right = null;
            if (p.PropertyType == typeof(int))
            {
                int val = Convert.ToInt32(value);
                right = Expression.Constant(val, p.PropertyType);
            }
            else if (p.PropertyType == typeof(decimal))
            {
                decimal val = Convert.ToDecimal(value);
                right = Expression.Constant(val, p.PropertyType);
            }
            else if (p.PropertyType == typeof(byte))
            {
                byte val = Convert.ToByte(value);
                right = Expression.Constant(val, p.PropertyType);
            }
            else
            {
                right = Expression.Constant(value, p.PropertyType);
            }
            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            Expression left = Expression.Property(param, p.Name);
            Expression filter = operateExpression(left, right);
            Expression<Func<T, bool>> pred = Expression.Lambda<Func<T, bool>>(filter, param);
            source = source.Where(pred);
            return source;
        }

        ///<summary>
        ///字符串包含操作
        ///</summary>
        ///<param name="left"></param>
        ///<param name="right"></param>
        ///<returns></returns>
        public static Expression StringContains(Expression left, Expression right)
        {
            Expression filter = Expression.Call(left, typeof(string).GetMethod("Contains"), right);
            return filter;
        }
        #endregion

        #region 模型
        //数据过滤模型类
        public class DataFilterModel
        {
            public string field { get; set; }
            public string value { get; set; }
            public string op { get; set; }
        }
        //数据过滤操作枚举类
        private enum DataFliterOperatorTypeEnum
        {
            contains = 0,
            equal = 1,
            greater = 2,
            greaterorequal = 3,
            less = 4,
            lessorequal = 5,
        } 
        #endregion

    }

