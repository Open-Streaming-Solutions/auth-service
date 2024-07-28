using System.Reflection;
using AuthorizationService.Api;
using AuthorizationService.Api.Database;
using AuthorizationService.Api.DTOs.Requests;
using AuthorizationService.Api.Repositories;
using AuthorizationService.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Swashbuckle.AspNetCore.SwaggerGen;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Configure NLog
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Host.UseNLog();

// Configure SwaggerGen
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(ConfigureSwagger);

// Configure database
builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
	new NpgsqlConnectionFactory(config["Database:ConnectionString"]!));

// Configure Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
	options.Configuration = builder.Configuration["Cache:ConnectionString"];
	options.InstanceName = "AUTH_";
});
builder.Services.AddSingleton<ICachingService, CachingService>();

// Configure app repositories
builder.Services.AddSingleton<IAuthRepository, AuthRepository>();

// Configure app services
builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddSingleton<IUserService, UserServiceMock>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Initialize database
var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

// TODO: Refactor mappings
app.MapPost(ApiEndpointsUrls.Register, async ([FromBody] RegistrationRequest regData, 
		[FromServices] IAuthService authService) =>
	{
		var isRegistered = await authService.Register(regData);
		
		return isRegistered
			? Results.Created()
			: Results.Conflict();
	})
	.Produces(StatusCodes.Status201Created)
	.Produces(StatusCodes.Status409Conflict)
	.WithName(ApiEndpointsUrls.Register)
	.WithOpenApi();

app.MapPost(ApiEndpointsUrls.LoginBase, async ([FromBody] AuthorizationRequest authData, 
		[FromServices] IAuthService authService) =>
	{
		var regInfo = await authService.Login(authData);
		
		return regInfo is not null
			? TypedResults.Ok(regInfo)
			: Results.NotFound();
	})
	.Produces(StatusCodes.Status200OK)
	.Produces(StatusCodes.Status404NotFound)
	.WithName(ApiEndpointsUrls.LoginBase)
	.WithOpenApi();

app.MapGet(ApiEndpointsUrls.InitLogin, async ([FromQuery] string username, 
		[FromServices] IAuthService authService) =>
	{
		var regInfo = await authService.InitLogin(username);
		
		// TODO: Return authorization type
		return regInfo is not null
			? TypedResults.Ok(regInfo)
			: Results.NotFound();
	})
	.Produces(StatusCodes.Status200OK)
	.Produces(StatusCodes.Status409Conflict)
	.WithName(ApiEndpointsUrls.InitLogin)
	.WithOpenApi();

await app.RunAsync();
	
static void ConfigureSwagger(SwaggerGenOptions options)
{
	options.SwaggerDoc("v1", new OpenApiInfo
	{
		Version = "v1",
		Title = "Authorization service API",
		License = new OpenApiLicense
		{
			Name = "MIT Licence"
		}
	});
	
	var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
	options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
}
