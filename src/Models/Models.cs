using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ChuntianCms.Models;

#region 登录首页
public class AccountModel
{
    public int Id { get; set; }
    public string TrueName { get; set; }
    public string UserName { get; set; }
    public string RoleName { get; set; }
    public int RoleId { get; set; }
    public string ExpiresTime { get; set; }
    public string Logo { get; set; }
    public string LocalIp { get; set; }

}

public class PermModel
{
    public string KeyCode { get; set; }
}
public class UserPwdSetViewModel
{
    public string OldPwd { get; set; }
    public string NewPwd { get; set; }
    public string AgainPWd { get; set; }
}
public class SystemSettingViewModel
{
    public string SiteName { get; set; }
    public string Domain { get; set; }
    public int Cache { get; set; } = 0;
    public int MaxFileSize { get; set; } = 2048;
    public string FileType { get; set; } = "png|gif|jpg|jpeg|zip|rar";
    public string Title { get; set; }
    public string Speak { get; set; }
    public string CopyRight { get; set; }
}
public class SystemViewModel
{
    public string UserName { get; set; }
    public string RoleName { get; set; }
    public string TrueName { get; set; }
    public string ExpiresTime { get; set; }
    public string LocalIp { get; set; }
    public string ServerIp { get; set; }

}
public class SiteDataViewModel
{
    public int SiteCount { get; set; }
    public int ArticleCount { get; set; }
    public int UserCount { get; set; }
    public int ProCount { get; set; }
}
public class IpCountViewModel
{
    public int Count { get; set; }
    public string TypeName { get; set; }
    public DateTime CreatedTime { get; set; }
    public string NewId { get; set; }
}
#endregion

#region 权限
public class SysRight
{
    public DateTime? CreatedTime { get; set; }
    public string Id { get; set; }
    public int ModuleId { get; set; }
    public bool? Rightflag { get; set; }
    public int RoleId { get; set; }
    public DateTime? UpdatedTime { get; set; }
}
#endregion

#region 角色
public class SysRole
{

    public int Id { get; set; }
    public string NewId { get; set; }
    public string Name { get; set; }
    public string CreatePerson { get; set; }
    public string Description { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }

}
#endregion

#region 用户
public class SysUser
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Logo { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public string TrueName { get; set; }
    public DateTime? CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }

}
#endregion

#region 菜单
public class SysMenu
{

    public int Id { get; set; }
    public string NewId { get; set; }
    public string Target { get; set; }
    public int? IsMenu { get; set; }
    public string MenuUrl { get; set; }
    public string Name { get; set; }
    public int ParentId { get; set; }
    public int? Sort { get; set; }
    public string Speak { get; set; }
    public string Code { get; set; }
    public DateTime? CreateTime { get; set; }
    public DateTime? UpdateTime { get; set; }
    public int? Checked { get; set; }
    public string MenuIcon { get; set; }

}
#endregion

#region 内容站
public class SiteNews
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int TypeId { get; set; }
    public string TypeName { get; set; }
    public string Context { get; set; }
    public int Hits { get; set; }
    public string CreatedTime { get; set; }
    public string UpdatedTime { get; set; }
}
public class SiteType
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CreatedTime { get; set; }
    public string UpdatedTime { get; set; }

}
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Score { get; set; }
    public string CreatedTime { get; set; }
}
#endregion
