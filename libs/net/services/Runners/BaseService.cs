﻿using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TNO.Core.Http;
using TNO.CSS;
using TNO.CSS.Config;
using TNO.Services.Config;
using TNO.Services.Controllers;

namespace TNO.Services.Runners;

/// <summary>
/// BaseService abstract class, provides a console application that runs a service, and an api.
/// The main purpose of the BaseService is to configure common dependency injection.
/// </summary>
public abstract class BaseService
{
    #region Variables
    /// <summary>
    /// The logger for the service.
    /// </summary>
    private readonly ILogger _logger;
    #endregion

    #region Properties
    /// <summary>
    /// get - Program configuration.
    /// </summary>
    public IConfiguration Configuration { get; private set; }

    /// <summary>
    /// get - The service provider.
    /// </summary>
    public WebApplication App { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a BaseService object, initializes with arguments.
    /// </summary>
    /// <param name="args"></param>
    public BaseService(string[] args)
    {
        DotNetEnv.Env.Load();
        var builder = WebApplication.CreateBuilder(args);
        this.Configuration = Configure(builder, args).Build();
        ConfigureServices(builder.Services);
        this.App = builder.Build();
        Console.OutputEncoding = Encoding.UTF8;
        _logger = this.App.Services.GetRequiredService<ILogger<BaseService>>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Configure application.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    protected virtual IConfigurationBuilder Configure(WebApplicationBuilder builder, string[]? args)
    {
        string? environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "http://+:5000";
        builder.WebHost.UseUrls(urls);
        return builder.Configuration
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args ?? Array.Empty<string>());
    }

    /// <summary>
    /// Configure dependency injection.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    protected virtual IServiceCollection ConfigureServices(IServiceCollection services)
    {
        var jsonSerializerOptions = new JsonSerializerOptions()
        {
            DefaultIgnoreCondition = !String.IsNullOrWhiteSpace(this.Configuration["Serialization:Json:DefaultIgnoreCondition"]) ? Enum.Parse<JsonIgnoreCondition>(this.Configuration["Serialization:Json:DefaultIgnoreCondition"]) : JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = !String.IsNullOrWhiteSpace(this.Configuration["Serialization:Json:PropertyNameCaseInsensitive"]) && Boolean.Parse(this.Configuration["Serialization:Json:PropertyNameCaseInsensitive"]),
            PropertyNamingPolicy = this.Configuration["Serialization:Json:PropertyNamingPolicy"] == "CamelCase" ? JsonNamingPolicy.CamelCase : null,
            WriteIndented = !string.IsNullOrWhiteSpace(this.Configuration["Serialization:Json:WriteIndented"]) && Boolean.Parse(this.Configuration["Serialization:Json:WriteIndented"])
        };

        services
            .AddSingleton<IConfiguration>(this.Configuration)
            .Configure<ServiceOptions>(this.Configuration.GetSection("Service"))
            .Configure<JsonSerializerOptions>(options =>
            {
                options.DefaultIgnoreCondition = jsonSerializerOptions.DefaultIgnoreCondition;
                options.PropertyNameCaseInsensitive = jsonSerializerOptions.PropertyNameCaseInsensitive;
                options.PropertyNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy;
                options.WriteIndented = jsonSerializerOptions.WriteIndented;
                options.Converters.Add(new JsonStringEnumConverter());
            })
            .AddLogging(options =>
            {
                options.AddConfiguration(this.Configuration.GetSection("Logging"));
                options.AddConsole();
            })
            .AddTransient<JwtSecurityTokenHandler>()
            .Configure<CssOptions>(this.Configuration.GetSection("CSS"))
            .AddTransient<IHttpRequestClient, HttpRequestClient>()
            .AddTransient<ICssClient, CssClient>()
            .AddTransient<IApiService, ApiService>();

        // TODO: Figure out how to validate without resulting in aggregating the config values.
        // services.AddOptions<AuthClientOptions>()
        //     .Bind(this.Configuration.GetSection("Auth:Keycloak"))
        //     .ValidateDataAnnotations();

        // services.AddOptions<OpenIdConnectOptions>()
        //     .Bind(this.Configuration.GetSection("Auth:OIDC"))
        //     .ValidateDataAnnotations();

        // services.AddOptions<ServiceOptions>()
        //     .Bind(this.Configuration.GetSection("Service"))
        //     .ValidateDataAnnotations();

        services.AddHttpClient(typeof(BaseService).FullName, client => { });

        // API services
        services.AddMvcCore()
            .AddApplicationPart(typeof(HealthController).Assembly);
        services.AddHttpContextAccessor()
            .AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = true;
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = jsonSerializerOptions.DefaultIgnoreCondition;
                options.JsonSerializerOptions.PropertyNameCaseInsensitive = jsonSerializerOptions.PropertyNameCaseInsensitive;
                options.JsonSerializerOptions.PropertyNamingPolicy = jsonSerializerOptions.PropertyNamingPolicy;
                options.JsonSerializerOptions.WriteIndented = jsonSerializerOptions.WriteIndented;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return services;
    }

    /// <summary>
    /// Run the ingestion service.
    /// </summary>
    /// <returns></returns>
    public async Task<int> RunAsync()
    {
        try
        {
            _logger.LogInformation("Service started");
            var tasks = new[] { RunApiAsync(), RunServiceAsync() };
            var task = await Task.WhenAny(tasks);
            if (task.Status == TaskStatus.Faulted) throw task.Exception ?? new Exception("An unhandled exception has occured.");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "An unhandled error has occurred.");
            return 1;
        }
        finally
        {
            _logger.LogInformation("Service stopped");
        }
    }

    /// <summary>
    /// Run the service.
    /// </summary>
    /// <returns></returns>
    private async Task RunServiceAsync()
    {
        var service = this.App.Services.GetRequiredService<IServiceManager>();
        await service.RunAsync();
    }

    /// <summary>
    /// Run the API.
    /// </summary>
    private async Task RunApiAsync()
    {
        // Configure the HTTP request pipeline.
        if (this.App.Environment.IsDevelopment())
        {
            this.App.UseDeveloperExceptionPage();
        }

        this.App.UsePathBase(this.Configuration.GetValue<string>("BaseUrl"));
        this.App.UseForwardedHeaders();

        // app.UseHttpsRedirection();
        this.App.UseRouting();
        this.App.UseCors();

        this.App.MapControllers();

        await this.App.RunAsync();
    }
    #endregion
}
