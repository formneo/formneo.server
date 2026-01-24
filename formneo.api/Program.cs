using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Web.Services.Description;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using formneo.api.Controllers.MyApplication.Services;
using formneo.api.Filters;
using formneo.api.Helper;
using formneo.api.Middlewares;
using formneo.api.Modules;
using formneo.core.Configuration;
using formneo.core.Models;
using formneo.core.Repositories;
using formneo.core.Services;
using formneo.core.UnitOfWorks;
using formneo.core.Options;
using formneo.repository;
using formneo.repository.Repositories;
using formneo.repository.UnitOfWorks;
using formneo.service.Mapping;
using formneo.service.Services;
using formneo.service.SignService;
using formneo.service.Validations;
using formneo.service.Services;
using formneo.core.Configuration;
using ServiceCollection = Microsoft.Extensions.DependencyInjection.ServiceCollection;
using formneo.api.Seed;
using FluentValidation.AspNetCore;
using formneo.core.Services;
using formneo.api.Helper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;




});
builder.Services.AddHttpClient<EmployeeLeaveApiService>()
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
    });
builder.Services.Configure<EmployeeLeaveApiService>(
    builder.Configuration.GetSection("SapConnectionInfo"));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Bearer Token girin. Örnek: 'Bearer {token}'"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});



// Türkçe localization ayarı
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "tr-TR", "en-US" };
    options.SetDefaultCulture("tr-TR");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ValidateFilterAttribute());
    // SF* controller'ları devre dışı bırak
}).AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<EmployeeDtoValidator>());


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
//builder.Services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
//builder.Services.AddScoped(typeof(IService<>), typeof(Service<>));


var serviceProvider = new ServiceCollection()
    .AddMemoryCache() // MemoryCache servisini ekliyoruz
    .AddSingleton<MyCacheService>() // MyCacheService'i ekliyoruz
    .BuildServiceProvider();



builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys")))
    .SetApplicationName("formneo");



builder.Services.AddIdentity<UserApp, IdentityRole>(Opt =>
{
    Opt.User.RequireUniqueEmail = true;
    Opt.Password.RequireNonAlphanumeric = false;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

builder.Services.AddHttpClient<EmployeeLeaveApiService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
var dataProtectionKeysPath = OperatingSystem.IsWindows() ? @"C:\\temp-keys" : "/tmp/keys";
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });

builder.Services.AddDataProtection()
    .SetApplicationName("formneo");

builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicAuthentication", new AuthorizationPolicyBuilder("BasicAuthentication").RequireAuthenticatedUser().Build());
});


//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
//{
//    var tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
//    opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
//    {
//        ValidIssuer = tokenOptions.Issuer,
//        ValidAudience = tokenOptions.Audience[0],
//        IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),

//        ValidateIssuerSigningKey = true,
//        ValidateAudience = true,
//        ValidateIssuer = true,
//        ValidateLifetime = true,
//        ClockSkew = TimeSpan.Zero
//    };
//});


builder.Services.AddCors(c =>
        {
        c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin());
        });

        builder.Services.AddCors(policyBuilder =>
        policyBuilder.AddDefaultPolicy(policy =>
        policy.WithOrigins("*").AllowAnyHeader().AllowAnyHeader().AllowAnyMethod()));

        builder.Services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
        {
            builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowAnyOrigin();

        }));


        builder.Services.AddCors(opt =>
        {
        opt.AddPolicy("CORS", policy =>
        {
        policy.WithOrigins("http://localhost:8080", "https://api.formneo-tech.com")
        .AllowAnyHeader()
        .AllowCredentials()
        .AllowAnyMethod();

        });
        });

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IMinioService, MinioService>();
builder.Services.AddScoped<IMailService, formneo.service.Services.MailService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddHttpClient<QuestDBService>();

AppContext.SetSwitch("System.Drawing.EnableUnixSupport", true);

builder.Services.AddScoped(typeof(NotFoundFilter<>));
builder.Services.AddAutoMapper(typeof(MapProfile));
builder.Services.Configure<CustomTokenOption>(builder.Configuration.GetSection("TokenOption"));
builder.Services.Configure<RoleScopeOptions>(builder.Configuration.GetSection("RoleScope"));
builder.Services.Configure<GetirOptions>(builder.Configuration.GetSection("Getir"));

//builder.Services.AddDbContext<AppDbContext>(x =>
//{
//    x.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"), option =>
//    {
//        option.MigrationsAssembly(Assembly.GetAssembly(typeof(AppDbContext)).GetName().Name);
//    });
//});


builder.Services.AddScoped<formneo.api.Filters.GlobalEntityWriteInterceptor>();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"), npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(Assembly.GetAssembly(typeof(AppDbContext)).GetName().Name);
    });
    options.AddInterceptors(sp.GetRequiredService<formneo.api.Filters.GlobalEntityWriteInterceptor>());
});
IdentityModelEventSource.ShowPII = true; //Add this line


////burasıydı
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}";
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidIssuer = builder.Configuration["AzureAd:Issuer"], // Token'deki iss ile eşleşmeli
//            ValidateAudience = true,
//            ValidAudience = builder.Configuration["AzureAd:Audience"], // Token'deki aud ile eşleşmeli
//            ValidateLifetime = true, // Token'in süresini kontrol eder
//        };
//    });

//    builder.Services.AddAuthentication(options =>
//    {
//        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opts =>
//    {
//        var tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
//        opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters()
//        {
//            ValidIssuer = tokenOptions.Issuer,
//            ValidAudience = tokenOptions.Audience[0],
//            IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),

//            ValidateIssuerSigningKey = true,
//            ValidateAudience = true,
//            ValidateIssuer = true,
//            ValidateLifetime = true,
//            ClockSkew = TimeSpan.Zero
//        };
//    });


//multi
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "MultiAuth";  // Varsayılan olarak MultiAuth kullan
    options.DefaultChallengeScheme = "MultiAuth";
})
    .AddPolicyScheme("MultiAuth", JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

            if (!string.IsNullOrEmpty(authHeader))
            {
                if (authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                {
                    return "BasicAuthentication";
                }
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    try
                    {
                        var parts = token.Split('.');
                        if (parts.Length == 3)
                        {
                            string Base64UrlDecode(string input)
                            {
                                string output = input.Replace('-', '+').Replace('_', '/');
                                switch (output.Length % 4)
                                {
                                    case 2: output += "=="; break;
                                    case 3: output += "="; break;
                                }
                                var bytes = Convert.FromBase64String(output);
                                return Encoding.UTF8.GetString(bytes);
                            }

                            var payloadJson = Base64UrlDecode(parts[1]);
                            using var doc = JsonDocument.Parse(payloadJson);
                            var root = doc.RootElement;
                            var iss = root.TryGetProperty("iss", out var issProp) ? issProp.GetString() : null;
                            var aud = root.TryGetProperty("aud", out var audProp) ? audProp.GetString() : null;

                            var azureIssuer = builder.Configuration["AzureAd:Issuer"];
                            var azureAudience = builder.Configuration["AzureAd:Audience"];

                            if ((!string.IsNullOrEmpty(iss) && iss == azureIssuer) || (!string.IsNullOrEmpty(aud) && aud == azureAudience))
                            {
                                return "AzureAd";
                            }
                        }
                    }
                    catch
                    {
                        // ignore and fall back to default JWT
                    }
                    return JwtBearerDefaults.AuthenticationScheme;
                }
            }

            return JwtBearerDefaults.AuthenticationScheme;
        };
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        var tokenOptions = builder.Configuration.GetSection("TokenOption").Get<CustomTokenOption>();
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuer = tokenOptions.Issuer,
            ValidAudience = tokenOptions.Audience[0],
            IssuerSigningKey = SignService.GetSymmetricSecurityKey(tokenOptions.SecurityKey),
            ValidateIssuerSigningKey = true,
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    })
    .AddJwtBearer("AzureAd", options =>
    {
        options.Authority = $"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AzureAd:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AzureAd:Audience"],
            ValidateLifetime = true,
            NameClaimType = "preferred_username"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicOnly", policy =>
    {
        policy.AddAuthenticationSchemes("BasicAuthentication");
        policy.RequireAuthenticatedUser();
    });
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes("MultiAuth") // Hem JWT hem de Azure AD desteği
        .RequireAuthenticatedUser()
        .Build();
});

// Permission policy provider & handler
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
builder.Services.AddScoped<IPermissionEvaluator, PermissionEvaluator>();

builder.Services.AddScoped<DbNameHelper>();
builder.Services.AddScoped<formneo.core.Services.ITenantContext, formneo.service.Services.TenantContext>();
builder.Host.UseServiceProviderFactory
    (new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder => containerBuilder.RegisterModule(new RepoServiceModule()));
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder => containerBuilder.RegisterModule(new GetirModule()));


var app = builder.Build();
    app.UseCors("MyPolicy");


//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{





app.UseSwagger();
    app.UseSwaggerUI();
//}





if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Statik dosyalar için wwwroot klasörünü kullan
app.UseStaticFiles();

// Default files (index.html) desteği
app.UseDefaultFiles();

app.UseCustomException();

// Türkçe localization middleware
app.UseRequestLocalization();

app.UseAuthentication();

app.UseMiddleware<formneo.api.Middlewares.TenantResolutionMiddleware>();

app.UseAuthorization();



app.UseDeveloperExceptionPage();

app.MapControllers();

// İlk çalıştırmada veritabanını ve admin kullanıcıyı oluştur
// formneo.api.Filters.GlobalEntityWriteInterceptor.SkipEnforcement = true;
// try
// {
//     await formneo.api.Seed.DatabaseInitializer.InitializeAsync(app.Services);
// }
// finally
// {
//     formneo.api.Filters.GlobalEntityWriteInterceptor.SkipEnforcement = false;
// }
formneo.api.Filters.GlobalEntityWriteInterceptor.SkipEnforcement = false;

app.Run();
