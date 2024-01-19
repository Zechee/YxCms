using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.Configuration;

namespace ChuntianCms.Helper;

/// <summary>
/// 配置管理器，用于读取配置文件内容
/// </summary>
public static class ConfigMgr
{
    /// <summary>
    /// 配置内容
    /// </summary>
    /// <value></value>
    public static IConfiguration Config { get; }
    static ConfigMgr()
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        env = string.IsNullOrEmpty(env) ? "" : "." + env;
        var settingFile = $"appsettings{env}.json";

        Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(path: settingFile, optional: false, reloadOnChange: true)
            .Build();
    }
}
