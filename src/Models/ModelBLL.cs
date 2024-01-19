using System.Collections.Generic;
using System;
using System.Linq;

namespace ChuntianCms.Models;


/// <summary>
/// json帮助类
/// </summary>
public class JsonHandler
{

    public static JsonMessage CreateMessage(int ptype, string pmessage, string pvalue)
    {
        JsonMessage json = new JsonMessage()
        {
            type = ptype,
            message = pmessage,
            value = pvalue
        };
        return json;
    }
    public static JsonMessage CreateMessage(bool ptype)
    {
        JsonMessage json = new JsonMessage()
        {
            type = ptype ? 1 : 0,
            message = ptype ? "成功" : "失败",
        };
        return json;
    }
    public static JsonMessage CreateMessage(int ptype)
    {
        JsonMessage json = new JsonMessage()
        {
            type = ptype,
            message = ptype == 1 ? "成功" : "失败",
        };
        return json;
    }
    public static JsonMessage CreateMessage(int ptype, string pmessage)
    {
        JsonMessage json = new JsonMessage()
        {
            type = ptype,
            message = pmessage,
        };
        return json;
    }
    public static JsonMessage CreateMessage(int ptype, string pmessage, object[] pvalue)
    {
        JsonMessage json = new JsonMessage()
        {
            type = ptype,
            message = pmessage,
            values = pvalue
        };
        return json;
    }
}
public class ErrorMsg : List<string>
{
    /// <summary>
    /// 添加错误
    /// </summary>
    /// <param name="errorMessage">信息描述</param>
    public new void Add(string errorMessage)
    {
        base.Add(errorMessage);
    }
    /// <summary>
    /// 获取错误集合
    /// </summary>
    public string MsgStr
    {
        get
        {
            string error = "";
            this.All(a =>
            {
                error += a + "，";
                return true;
            });
            return error;
        }
    }
}
public class PagerModel
{
    public int Rows { get; set; }//每页行数
    public int Page { get; set; }//当前页是第几页
    public string Order { get; set; }//排序方式
    public string Sort { get; set; }//排序列
    public string Query
    {
        get; set;
    }
    public string Where { get; set; }
    public string Typeids { get; set; }
    public int TotalRows { get; set; }
    public int TotalPages
    {
        get
        {
            return (int)Math.Ceiling(TotalRows / (float)Rows);
        }
    }
}
public class JsonModel
{
    public int code { get; set; }
    public string msg { get; set; }
    public int count { get; set; }
    public IEnumerable<object> data { get; set; }
}
public class SelectModels
{
    public SelectModel[] data { get; set; }

}
public class SelectModel
{
    public string selected { get; set; }
    public string name { get; set; }
    public string value { get; set; }

}
public class JsonMessage
{
    public int type { get; set; }
    public string message { get; set; }
    public string value { get; set; }
    public object[] values { get; set; }
}
