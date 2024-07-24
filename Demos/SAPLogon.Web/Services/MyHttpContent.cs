public class MyHttpContext {
    private static IHttpContextAccessor m_httpContextAccessor;

    public static HttpContext Current => m_httpContextAccessor.HttpContext;

    public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";
    public static string Host => Current.Request.Host.Value;
    public static string Path => Current.Request.Path;
    public static string PathAndQuery => $"{Current.Request.Path}{Current.Request.QueryString}";

    public static HttpRequest Request => Current.Request;
    public static HttpResponse Response => Current.Response;
    public static string Domain => GetDomainFromHost(Host);

    internal static void Configure(IHttpContextAccessor contextAccessor) {
        m_httpContextAccessor = contextAccessor;
    }

    private static string GetDomainFromHost(string hostValue) {
        string[] values = hostValue.Split('.');
        return values.Length >= 2 ? $"{values[^2]}.{values[^1]}" : "";
    }
}

public static class HttpContextExtensions {
    public static void AddHttpContextAccessor(this IServiceCollection services) {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    }

    public static IApplicationBuilder UseHttpContext(this IApplicationBuilder app) {
        MyHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
        return app;
    }
}