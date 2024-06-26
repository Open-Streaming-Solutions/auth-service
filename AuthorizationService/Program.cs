using System.Reflection;
using AuthorizationService.Database;
using AuthorizationService.DTOs.Requests;
using AuthorizationService.Repositories;
using AuthorizationService.Services;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Swashbuckle.AspNetCore.SwaggerGen;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace AuthorizationService;

public static class Program
{
	public static async Task Main(string[] args)
	{
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
		app.MapPost(ApiEndpointsUrls.RegisterBase, async (RegistrationRequest regData, 
				IAuthService authService) =>
			{
				var isRegistered = await authService.Register(regData);
				
				return isRegistered
					? TypedResults.Ok()
					: Results.Conflict();
			})
			.Produces(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status409Conflict)
			.WithName("Register")
			.WithOpenApi();
		
		app.MapPost(ApiEndpointsUrls.LoginBase, async (AuthorizationRequest authData, 
				IAuthService authService) =>
			{
				var regInfo = await authService.Login(authData);
				
				return regInfo != null
					? TypedResults.Ok(regInfo)
					: Results.Conflict();
			})
			.Produces(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status409Conflict)
			.WithName("Login")
			.WithOpenApi();
		
		app.MapGet(ApiEndpointsUrls.InitLogin, async (string username, 
				IAuthService authService) =>
			{
				var regInfo = await authService.InitLogin(username);
				
				return regInfo != null
					? TypedResults.Ok(regInfo)
					: Results.NotFound();
			})
			.Produces(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status409Conflict)
			.WithName("LoginInit")
			.WithOpenApi();
		
		await app.RunAsync();
	}
	
	private static void ConfigureSwagger(SwaggerGenOptions options)
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
}
