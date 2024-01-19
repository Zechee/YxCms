public static class RouteExtensions
{
    public static IApplicationBuilder UseStaticHttpContext(this IApplicationBuilder app)
    {
        var serviceProvider = app.ApplicationServices.GetRequiredService<IServiceProvider>();
        HttpContextHelper.Configure(serviceProvider);

        // var webHostEnvironment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        // AspxHelper.Configure(webHostEnvironment);

        // ConfigHelper.hostingEnvironment = webHostEnvironment;
        return app;
    }
}