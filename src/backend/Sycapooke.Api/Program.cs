using Microsoft.AspNetCore.Diagnostics;
using Sycapooke.Api.Auth;
using Sycapooke.Api.Http;
using Sycapooke.Api.Privacy;
using Sycapooke.Application;
using Sycapooke.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSycapookeAuth(builder.Configuration);
builder.Services.Configure<StrictlyNecessaryCookieOptions>(builder.Configuration.GetSection(StrictlyNecessaryCookieOptions.Section));

builder.Services.AddSingleton<IRateLimitStore, InMemoryRateLimitStore>();
builder.WebHost.ConfigureKestrel(kestrel => kestrel.Limits.MaxRequestBodySize = 1_048_576);

var app = builder.Build();

if (app.Services.GetRequiredService<Sycapooke.Application.Shop.ShopOptions>().FounderMemberId is null)
{
    app.Logger.LogWarning("Shop:FounderMemberId is not set, so nobody can manage the shop. See docs/deployment.md, Provision the founder.");
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseExceptionHandler(new ExceptionHandlerOptions { ExceptionHandler = ExceptionHandling.WriteAsync });
app.UseMiddleware<CookieAllowListMiddleware>();
app.UseMiddleware<RateLimitingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>Entry point, exposed to WebApplicationFactory in Sycapooke.Api.Tests.</summary>
public partial class Program;
