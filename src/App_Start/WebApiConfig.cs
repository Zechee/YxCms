//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web.Http;

//namespace ChuntianCms
//{
//    public static class WebApiConfig
//    {
//        public static void Register(HttpConfiguration config)
//        {
//            // Web API 配置和服务
         
//            // Web API 路由
//            config.MapHttpAttributeRoutes();

//            config.Routes.MapHttpRoute(
//                name: "DefaultApi",
//                routeTemplate: "api/{controller}/{action}/{id}",
//                defaults: new { id = RouteParameter.Optional }
//            );
//            GlobalConfiguration.Configuration.EnsureInitialized();
//            // 取消注释下面的代码行可对具有 IQueryable 或 IQueryable<T> 返回类型的操作启用查询支持。
//            // 若要避免处理意外查询或恶意查询，请使用 QueryableAttribute 上的验证设置来验证传入查询。
//            // 有关详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=279712。
//            //config.EnableQuerySupport();
//        }
//    }
   
//}