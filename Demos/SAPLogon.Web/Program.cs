using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace SAPLogon.Web;

public class Program {
    //private static readonly string[] middleware = ["Accept-Encoding"];

    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddResponseCaching();
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddResponseCompression(options => {
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider()
                .Mappings.Select(m => m.Value).Distinct();
            options.EnableForHttps = true;
        });
        builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = CompressionLevel.Fastest);
        builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.SmallestSize);

        WebApplication app = builder.Build();
        if(!app.Environment.IsDevelopment()) {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseCookiePolicy();
        app.UseRouting();
        app.UseAuthorization();
        if(!app.Environment.IsDevelopment()) {
            app.UseResponseCompression();
            app.UseResponseCaching();
            app.UseStaticFiles(new StaticFileOptions {
                OnPrepareResponse = ctx => ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000")
            });
        } else {
            app.UseStaticFiles();
        }
        app.MapRazorPages();
        app.Run();
    }
}