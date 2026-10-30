using Sycapooke.Api.Auth;
using Sycapooke.Api.Privacy;
using Sycapooke.Application;
using Sycapooke.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSycapookeAuth(builder.Configuration);
builder.Services.Configure<StrictlyNecessaryCookieOptions>(builder.Configuration.GetSection(StrictlyNecessaryCookieOptions.Section));

var app = builder.Build();

app.UseMiddleware<CookieAllowListMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

/// <summary>Entry point, exposed to WebApplicationFactory in Sycapooke.Api.Tests.</summary>
public partial class Program;
