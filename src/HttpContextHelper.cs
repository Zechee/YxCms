using System;

using Microsoft.AspNetCore.Http;


/// <summary>
/// Temporary helper class for retrieving the current <see cref="HttpContext"/> . This temporary
/// workaround should be removed in the future and <see cref="HttpContext"/> should be retrieved
/// from the current controller, middleware, or page instead.
///
/// If working in another component, the current <see cref="HttpContext"/> can be retrieved from an <see cref="IHttpContextAccessor"/>
/// retrieved via dependency injection.
/// </summary>
public static class HttpContextHelper
{
    private static IServiceProvider _serviceProvider;
    /// <summary>
    /// Gets the current <see cref="HttpContext"/>. Returns <c>null</c> if there is no current <see cref="HttpContext"/>.
    /// </summary>
    public static HttpContext Current
    {
        get
        {
            var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
            return httpContextAccessor!.HttpContext!;
        }
    }

    public static void Configure(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
}

