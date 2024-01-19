using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YutianData.DAL
{
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
