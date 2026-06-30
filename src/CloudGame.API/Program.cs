using CloudGame.API.Handlers;
using CloudGame.Application.Extensions;
using CloudGame.Application.Handlers.UserHandler.Create;
using CloudGame.Application.Settings;
using CloudGame.Domain.Commom;
using CloudGame.Domain.Events.User;
using CloudGame.Infrastructure.EntityFramework;
using CloudGame.Infrastructure.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using System.Data;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting up the application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((hostingContext, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(hostingContext.Configuration);
    });

    builder.Services.AddHealthChecks();

    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();

    builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .SelectMany(s => s.Value!.Errors
                .Select(v => new Error(s.Key, v.ErrorMessage)))
                .ToList();

            return new BadRequestObjectResult(Result.Failure(errors));
        };
    });

    builder.Services.AddOpenApi();

    builder.Services
        .AddApplicationHandlers()
        .AddInfrastructureServices(builder.Configuration);

    var jwtSettingsSection = builder.Configuration.GetRequiredSection("JwtSettings");
    builder.Services.Configure<JwtSettings>(jwtSettingsSection);

    var encriptKey = jwtSettingsSection.GetValue<string>("EncriptKey")!;
    var key = Encoding.ASCII.GetBytes(encriptKey);
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });


    builder.Services.AddExceptionHandler<CloudGameExceptionHandler>();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "TC - Fiap Cloud Game API",
            Version = "v1"
        });

        options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter JWT token like: Bearer {your token}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("bearer", document)] = []
        });
    });

    builder.Services.AddMassTransit(bus =>
    {
        bus.UsingRabbitMq((ctx, cfg) =>
        {
            var rabbitMqSection = builder.Configuration.GetRequiredSection("RabbitMQ")!;
            var host = rabbitMqSection["Host"]!;
            var username = rabbitMqSection["Username"]!;
            var password = rabbitMqSection["Password"]!;

            cfg.Host(host, "/", h =>
            {
                h.Username(username);
                h.Password(password);
            });

            cfg.ConfigureEndpoints(ctx);

            //cfg.Publish<UserCreatedEvent>(p =>
            //{
            //    p.ExchangeType = RabbitMQ.Client.ExchangeType.Direct;
            //});
        });
    });

    var app = builder.Build();

    Log.Information("The application has been built, and star the pipeline setup has started.");

    await using (var scope = app.Services.CreateAsyncScope())
    await using (var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>())
    {
        await appDbContext.Database.MigrateAsync();
    }

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthorization();

    app.MapControllers();

    app.MapHealthChecks("/health");

    Log.Information("Pipeline successfully configured and application initialized...");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.Information("Shutting down the application...");
    Log.CloseAndFlush();
}
