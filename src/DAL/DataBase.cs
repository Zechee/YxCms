using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;

using Dapper;

namespace ChuntianCms.DAL;


public enum DataBaseType
{
    SqlServer,
    MySql,
    Sqlite
}

/// <summary>
/// 数据访问类基于dapper和原生sql打造密封类不允许继承和派生
/// </summary>
public sealed class DataBase
{
    #region 变量初始化
    private static string connStr = "";
    public DataBase()
    {
        connStr = ChuntianCms.Helper.ConfigMgr.Config.GetValue<string>("ConnectionStrings:DBSqlServerLocalStr");
    }
    #endregion

    #region 非静态方法实例化类之后
    /// <summary>
    /// 创建一个打开的链接
    /// </summary>
    /// <returns></returns>
    public SqlConnection CreateConnection()
    {
        var conn = new SqlConnection(connStr);
        conn.Open();
        return conn;
    }
    /// <summary>
    /// 执行sql语句增删查改等
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parms"></param>
    /// <returns></returns>
    public int Execute(string sql, object? parms = null)
    {
        using IDbConnection conn = CreateConnection();

        return conn.Execute(sql, parms);
    }
    /// <summary>
    /// 得到第一行第一列的唯一值
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parms"></param>
    /// <returns></returns>
    public object ExecuteScalar(string sql, object? parms = null)
    {
        using IDbConnection conn = CreateConnection();

        return conn.ExecuteScalar(sql, parms);
    }
    /// <summary>
    /// 根据lamada表达式和sql语句实现where条件查询
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="sql"></param>
    /// <param name="pre"></param>
    /// <param name="parms"></param>
    /// <returns>IQueryable接口的集合</returns>
    public IQueryable<TEntity> Query<TEntity>(string sql, object? parms = null, Expression<Func<TEntity, bool>>? pre = null)
    {
        using (IDbConnection conn = CreateConnection())
        {
            return pre != null ? conn.Query<TEntity>(sql, parms).AsQueryable().Where(pre) : conn.Query<TEntity>(sql, parms).AsQueryable();
        }
    }

    /// <summary>
    /// 多个数据集查询返回多个表的联合查询的结果集合对象相当于datatable之类的匿名类型
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parms"></param>
    /// <returns></returns>
    public SqlMapper.GridReader MultyQuery(string sql, object? parms = null)
    {
        using (IDbConnection conn = CreateConnection())
        {
            return conn.QueryMultiple(sql, parms);
        }
    }
    /// <summary>
    /// 单个数据集查询获取第一条结果
    /// </summary>
    /// <param name="sql"></param>
    /// <param name="parms"></param>
    /// <returns></returns>
    public TEntity? FirstOrDefault<TEntity>(string sql, Func<TEntity, bool> selector, object? parms = null)
    {
        using (IDbConnection conn = CreateConnection())
        {
            return conn.Query<TEntity>(sql, parms).Where(selector).FirstOrDefault();
        }
    }
    public TEntity? LastOrDefault<TEntity>(string sql, Func<TEntity, bool> selector, object? parms = null)
    {
        using (IDbConnection conn = CreateConnection())
        {
            return conn.Query<TEntity>(sql, parms).Where(selector).LastOrDefault();
        }
    }
    public TEntity? SingleOrDefault<TEntity>(string sql, Func<TEntity, bool> selector, object? parms = null)
    {
        using (IDbConnection conn = CreateConnection())
        {
            return conn.Query<TEntity>(sql, parms).Where(selector).SingleOrDefault();
        }
    }
    #endregion

}
