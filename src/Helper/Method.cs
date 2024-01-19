using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Globalization;
using System.Reflection;
using System.Data;
using System.Timers;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

/// <summary>
/// 字符串文本处理技术，通用方法
/// </summary>

public class Method
{

    #region 获取真实ip
    /// <summary>
    /// 获取真实ip
    /// </summary>
    /// <returns></returns>
    public static string GetRealIP(HttpContext context)
    {
        string result = string.Empty;
        result = context.Request.Headers["HTTP_X_FORWARDED_FOR"];
        //可能有代理 
        if (!string.IsNullOrWhiteSpace(result))
        {
            //没有"." 肯定是非IP格式
            if (result.IndexOf(".") == -1)
            {
                result = null;
            }
            else
            {
                //有","，估计多个代理。取第一个不是内网的IP。
                if (result.IndexOf(",") != -1)
                {
                    result = result.Replace(" ", string.Empty).Replace("\"", string.Empty);
                    string[] temparyip = result.Split(",;".ToCharArray());
                    if (temparyip != null && temparyip.Length > 0)
                    {
                        for (int i = 0; i < temparyip.Length; i++)
                        {
                            //找到不是内网的地址
                            if (IsIPAddress(temparyip[i]) && temparyip[i].Substring(0, 3) != "10." && temparyip[i].Substring(0, 7) != "192.168" && temparyip[i].Substring(0, 7) != "172.16.")
                            {
                                return temparyip[i];
                            }
                        }
                    }
                }
                //代理即是IP格式
                else if (IsIPAddress(result))
                {
                    return result;
                }
                //代理中的内容非IP
                else
                {
                    result = null;
                }
            }
        }
        if (string.IsNullOrWhiteSpace(result))
        {
            result = context.Request.Headers["REMOTE_ADDR"];
        }
        if (string.IsNullOrWhiteSpace(result))
        {
            result = context.Request.HttpContext.Connection?.RemoteIpAddress?.ToString() ?? "";
        }
        return result;
    }
    private static bool IsIPAddress(string str)
    {
        //判断是否为IP方法2：Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        string num = @"(25[0-5]|2[0-4]\d|[0-1]\d{2}|[1-9]?\d)";
        return Regex.IsMatch(str,
        ("^" + num + "\\." + num + "\\." + num + "\\." + num + "$"));
    }
    #endregion

    public static string GetServerPath(HttpContext context, string path)
    {
        var hostEnviroment = context.RequestServices.GetService<IWebHostEnvironment>();
        return Path.Combine(hostEnviroment!.ContentRootPath, path);
    }
    /// <summary>
    /// 创建一个全球唯一的32位ID
    /// </summary>
    /// <returns>ID串</returns>
    public static string NewId
    {
        get
        {
            //string id = DateTime.Now.ToString("yyyyMMddHHmmssfffffff");
            //string guid = Guid.NewGuid().ToString().Replace("-", "");
            //id += guid.Substring(0, 10);
            return Guid.NewGuid().ToString();
        }
    }
    /// <summary>
    /// 数组绑定到select控件上
    /// </summary>
    /// <param name="arrs"></param>
    /// <returns></returns>
    public static List<dynamic> GetddlListByArray(IEnumerable<string> arr)
    {
        List<dynamic> list = new List<dynamic>();
        var arrs = arr.ToArray();
        for (int i = 0; i < arrs.Length; i++)
        {
            list.Add(new { Id = arrs[i], Name = arrs[i] });
        }
        return list;
    }
    /// <summary>
    /// 字符串转换为按照符号分割的数组
    /// </summary>
    /// <param name="str"></param>
    /// <param name="fuhao"></param>
    /// <returns></returns>
    public static string[] StringSplitToArray(string str, string fuhao = "\n")
    {
        return str.Trim().Split(new string[] { fuhao }, StringSplitOptions.RemoveEmptyEntries);
    }
    /// <summary>
    /// 32位MD5加密
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string GetMD5(string password)
    {
        string cl = password;
        string pwd = "";
        MD5 md5 = MD5.Create(); //实例化一个md5对像
                                // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
        byte[] s = md5.ComputeHash(Encoding.UTF8.GetBytes(cl));
        // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
        for (int i = 0; i < s.Length; i++)
        {
            // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符 
            pwd = pwd + s[i].ToString("X2");
        }
        return pwd;
    }

    /// <summary>
    /// 程序执行时间测试
    /// </summary>
    /// <param name="dateBegin">开始时间</param>
    /// <param name="dateEnd">结束时间</param>
    /// <returns>返回(秒)单位，比如: 0.00239秒</returns>
    public static string ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
    {
        TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
        TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
        TimeSpan ts = ts1.Subtract(ts2).Duration();
        string dateDiff = null;
        if (ts.Days > 0)
        {
            dateDiff = ts.Days.ToString() + "天" + ts.Hours.ToString() + "小时" + ts.Minutes.ToString() + "分钟" + ts.Seconds.ToString() + "秒";
        }
        else if (ts.Hours > 0)
        {
            dateDiff = ts.Hours.ToString() + "小时" + ts.Minutes.ToString() + "分钟" + ts.Seconds.ToString() + "秒";
        }
        else if (ts.Minutes > 0)
        {
            dateDiff = ts.Minutes.ToString() + "分钟" + ts.Seconds.ToString() + "秒";
        }
        else if (ts.Seconds > 0)
        {
            dateDiff = ts.Seconds.ToString() + "秒";

        }
        else
        {
            dateDiff = ts.TotalMilliseconds.ToString() + "毫秒";
        }
        return dateDiff;
    }


   


}



