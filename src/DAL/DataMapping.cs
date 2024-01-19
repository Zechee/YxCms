using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ChuntianCms.DAL
{
    /// <summary>
    /// 泛型加反射获取model属性拼接sql字符串实现orm映射的功能
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    internal class DataMapping<TModel> where TModel:class
    {
        #region 数据库上下文属性
        public static DataBase DbContext
        {
            get;set;
        }

        #endregion


        #region 主键属性字段+PrimaryKey
        /// <summary>
        /// 主键字段名称
        /// </summary>
        public static string PrimaryKey
        {
            get
            {
                Type t = typeof(TModel);
                TableInfoAttribute tableInfo = t.GetCustomAttribute(typeof(TableInfoAttribute), true) as TableInfoAttribute;
                if (tableInfo != null)//如果没有标识表信息特性，则通过表名向数据库中得到主键信息
                {
                    return tableInfo.PrimaryKey;
                }
                else
                {
                    string tableName = TableName();
                    return DbContext.ExecuteScalar("SELECT name FROM SysColumns WHERE id=Object_Id('" + tableName + "') and colid=(select top 1 colid from sysindexkeys where id=Object_Id('" + tableName + "'))").ToString();
                    
                }
            }
        }
        #endregion

        #region 获取表名+TableName
        /// <summary>
        /// 获取表名
        /// </summary>
        /// <param name="prev">数据库表名前缀</param>
        /// <returns></returns>
        public static string TableName(string prev = "")
        {
            Type t = typeof(TModel);
            TableInfoAttribute tableInfo = t.GetCustomAttribute(typeof(TableInfoAttribute), true) as TableInfoAttribute;
            return tableInfo != null ? tableInfo.TableName : string.Concat(prev, t.Name);
        }
        #endregion

        #region Select 查询语句+GetQuerySql
        /// <summary>
        /// Select 查询语句
        /// </summary>
        /// <returns></returns>
        public static string GetQuerySql()
        {
            StringBuilder sql = new StringBuilder("select * from ");
            sql.Append(TableName());
            return sql.ToString();
        }
        public static string GetSomeSql()
        {
            return string.Format(@"select * from {0} where {1} in @IdList", TableName(), PrimaryKey);
        }
        public static string GetQuerySqlFileds(string[] arrs)
        {
            string str = string.Join(",", arrs);
            str = "select " + str + " from " + TableName();
            return str;
        }
        public static string GetQuerySqlFilterFileds(string[] arrs)
        {
            string[] files = GetFiledsName();
            var next = files.Except(arrs);
            string str = string.Join(",", next);
            str = "select " + str + " from " + TableName();
            return str;
        }
        #endregion

        #region Insert非Null属性的对象实例 Sql 语句+GetInsertSql
        /// <summary>
        /// Insert 非Null属性的对象实例 Sql 语句
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetInsertSql(TModel model, bool isContainKey = false)
        {
            StringBuilder sql = new StringBuilder("insert into ");
            //;select @@IDENTITY
            string[] props = PropertysCreate(model);
            if (isContainKey)//是否需要包含主键写入
            {
                string[] id = { PrimaryKey };
                props = props.Concat(id).ToArray();
            }
            Console.WriteLine(props.Length);
            sql.Append(TableName());
            sql.Append("(");
            sql.Append(string.Join(",", props));
            sql.Append(") values(@");
            sql.Append(string.Join(",@", props));
            sql.Append(")");

            return sql.ToString();
        }
        public static string[] PropertysCreate(TModel model)
        {
            PropertyInfo[] props = typeof(TModel).GetProperties();
            List<string> list = new List<string>();
            string key = PrimaryKey;
            if (props != null && props.Length > 0)
            {
                foreach (PropertyInfo prop in props)
                {
                    //prop.GetValue(model, null) != null &&空值不更新不写入
                    if (!prop.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(prop.Name);
                    }
                }
            }

            return list.ToArray();
        }
        #endregion

        #region Delete Sql 语句+GetDeleteSql
        /// <summary>
        /// Delete Sql 语句
        /// </summary>
        /// <returns></returns>
        public static string GetDeleteSql()
        {
            return string.Format(@"delete from {0} where {1} in @IdList", TableName(), PrimaryKey);
        }
        public static string GetDeleteSqlOne()
        {
            return string.Format(@"delete from {0} where {1} = @Id", TableName(), PrimaryKey);
        }
        #endregion

        #region Update 非Null属性的对象实例 Sql语句+GetUpdateSql
        /// <summary>
        /// Update 非Null属性的对象实例 Sql语句
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string GetUpdateSql(TModel model)
        {
            StringBuilder sql = new StringBuilder("update ");
            string[] props = PropertysEdit(model);
            sql.Append(TableName());
            sql.Append(" set ");
            foreach (string propName in props)
            {
                sql.Append(propName + "=@" + propName + ",");
            }
            sql.Remove(sql.Length - 1, 1);
            sql.Append(" where " + PrimaryKey + "=@"+ PrimaryKey);
            return sql.ToString();
        }
        #endregion
       
        #region 非主键且非Null属性集合+Propertys
        /// <summary>
        /// 非主键且非Null属性
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string[] PropertysEdit(TModel model)
        {
            PropertyInfo[] props = typeof(TModel).GetProperties();
            List<string> list = new List<string>();
            string key = PrimaryKey;
            if (props != null && props.Length > 0)
            {
                foreach (PropertyInfo prop in props)
                {
                    //prop.GetValue(model, null) != null &&空值不更新不写入
                    if (!prop.Name.Equals(key, StringComparison.OrdinalIgnoreCase))
                    {
                        list.Add(prop.Name);
                    }
                }
            }

            return list.ToArray();
        }
        public static string[] GetFiledsName()
        {
            PropertyInfo[] props = typeof(TModel).GetProperties();
            List<string> list = new List<string>();
            if (props != null && props.Length > 0)
            {
                foreach (PropertyInfo prop in props)
                {
                    list.Add(prop.Name);
                }
            }
            return list.ToArray();
        }
        #endregion


        #region 反射拼接获取增加修改sql语句
        /// <summary>
        /// 获得主键名称
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private static string GetPK<T>() where T : class
        {
            string pkName = string.Empty;
            Type objTye = typeof(T);
            TableInfoAttribute pk;
            foreach (Attribute attr in objTye.GetCustomAttributes(true))
            {
                pk = attr as TableInfoAttribute;
                if (pk != null)
                    return pk.PrimaryKey;
            }
            return pkName;
        }

        /// <summary>
        /// sql修改语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entity"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string UpdateSql<T>(T entity, string tableName) where T : class
        {
            if (entity == null || string.IsNullOrEmpty(tableName))
            {
                return string.Empty;
            }

            string pkName = GetPK<T>();

            if (string.IsNullOrEmpty(pkName))
            {
                return string.Empty;
            }

            string pkValue = string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append("update ");
            sb.Append(tableName);
            sb.Append(" set ");
            Type type = entity.GetType();
            PropertyInfo[] props = type.GetProperties();
            List<string> paraList = new List<string>();
            foreach (var prop in props)
            {
                if (prop.Name == (string)pkName)
                {
                    pkValue = (string)prop.GetValue(entity);
                }
                else
                {
                    paraList.Add(GetUpdatePara(prop, entity));
                }
            }

            if (paraList.Count == 0)
            {
                return string.Empty;
            }

            sb.Append(string.Join(",", paraList));

            if (string.IsNullOrEmpty(pkValue))
            {
                throw new Exception("主键不能为空");
            }

            sb.Append(" where ");
            sb.Append(pkName);
            sb.Append(" = ");
            sb.AppendFormat("'{0}'", pkValue);

            return sb.ToString();
        }

        /// <summary>
        /// 获得修改参数
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static string GetUpdatePara<T>(PropertyInfo property, T entity)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(" {0}='{1}' ", property.Name, property.GetValue(entity));
            return sb.ToString();
        }
        /// <summary>
        /// Insert SQL语句
        /// </summary>
        /// <param name="obj">要转换的对象，不可空</param>
        /// <param name="tableName">要添加的表明，不可空</param>
        /// <returns>
        /// 空
        /// sql语句
        /// </returns>
        public static string InsertSql<T>(T t, string tableName) where T : class
        {
            if (t == null || string.IsNullOrEmpty(tableName))
            {
                return string.Empty;
            }
            string columns = GetColmons(t);
            if (string.IsNullOrEmpty(columns))
            {
                return string.Empty;
            }
            string values = GetValues(t);
            if (string.IsNullOrEmpty(values))
            {
                return string.Empty;
            }
            StringBuilder sql = new StringBuilder();
            sql.Append("Insert into " + tableName);
            sql.Append("(" + columns + ")");
            sql.Append(" values(" + values + ")");
            return sql.ToString();
        }

        /// <summary>
        /// BulkInsert SQL语句（批量添加）
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="objs">要转换的对象集合，不可空</param>
        /// <param name="tableName">>要添加的表明，不可空</param>
        /// <returns>
        /// 空
        /// sql语句
        /// </returns>
        public static string BulkInsertSql<T>(List<T> objs, string tableName) where T : class
        {
            if (objs == null || objs.Count == 0 || string.IsNullOrEmpty(tableName))
            {
                return string.Empty;
            }
            string columns = GetColmons(objs[0]);
            if (string.IsNullOrEmpty(columns))
            {
                return string.Empty;
            }
            string values = string.Join(",", objs.Select(p => string.Format("({0})", GetValues(p))).ToArray());
            StringBuilder sql = new StringBuilder();
            sql.Append("Insert into " + tableName);
            sql.Append("(" + columns + ")");
            sql.Append(" values " + values + "");
            return sql.ToString();
        }

        /// <summary>
        /// 获得类型的列名
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GetColmons<T>(T obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return string.Join(",", obj.GetType().GetProperties().Select(p => p.Name).ToList());
        }

        /// <summary>
        /// 获得值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static string GetValues<T>(T obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return string.Join(",", obj.GetType().GetProperties().Select(p => string.Format("'{0}'", p.GetValue(obj))).ToArray());
        }
        #endregion

    }

    #region 数据表特性类内部使用

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    /// <summary>
    /// 标识表名、主键等信息特性类
    /// </summary>
    public class TableInfoAttribute : Attribute
    {
        /// <summary>
        /// 数据库表名
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 主键名称
        /// </summary>
        public string PrimaryKey { get; set; }

        public TableInfoAttribute()
        { }
        public TableInfoAttribute(string tableName, string key)
        {
            this.TableName = tableName;
            this.PrimaryKey = key;
        }
    }
    #endregion

   
}
