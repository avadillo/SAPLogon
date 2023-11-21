namespace SAPLogon;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        _ = builder.Services.AddRazorPages();

        WebApplication app = builder.Build();
        if (!app.Environment.IsDevelopment()) {
            _ = app.UseExceptionHandler("/Error");
            _ = app.UseHsts();
        }
        _ = app.UseCookiePolicy();
        _ = app.UseStaticFiles();
        _ = app.UseRouting();
        _ = app.UseAuthorization();
        _ = app.MapRazorPages();
        app.Run();
    }
}