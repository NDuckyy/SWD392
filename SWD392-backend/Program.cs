using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using DotNetEnv;
using SWD392_backend.Context;
using SWD392_backend.Infrastructure.Repositories.OrderDetailRepository;
using SWD392_backend.Infrastructure.Repositories.OrderRepository;
using SWD392_backend.Infrastructure.Repositories.UserRepository;
using SWD392_backend.Infrastructure.Services.AuthService;
using SWD392_backend.Infrastructure.Services.OrderService;
using SWD392_backend.Infrastructure.Services.UserService;
using SWD392_backend.Infrastructure.Services.ProductService;
using SWD392_backend.Infrastructure.Repositories.ProductRepository;
using SWD392_backend.Infrastructure.Mappings;
using SWD392_backend.Infrastructure.Services.CategoryService;
using SWD392_backend.Infrastructure.Repositories.CategoryRepository;
using SWD392_backend.Infrastructure.Repositories.ProductImageRepository;
using SWD392_backend.Infrastructure.Services.ProductImageService;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql;
using StackExchange.Redis;
using SWD392_backend.Entities.Enums;
using SWD392_backend.Infrastructure.Repositories.SupplierRepository;
using SWD392_backend.Infrastructure.Services.S3Service;
using SWD392_backend.Infrastructure.Services.UploadService;
using SWD392_backend.Infrastructure.Services.ElasticSearchService;
using SWD392_backend.Infrastructure.Services.SupplerSerivce;
using SWD392_backend.Infrastructure.Services.ReviewService;
using SWD392_backend.Infrastructure.Repositories.ReviewRepository;
using SWD392_backend.Infrastructure.Services.ShipperService;
using SWD392_backend.Infrastructure.Repositories.ShipperRepository;

var builder = WebApplication.CreateBuilder(args);

// Load env
Env.Load();

// Create connection string
var connectionString = $"Host={Environment.GetEnvironmentVariable("DB_HOST")};" +
                       $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                       $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                       $"Username={Environment.GetEnvironmentVariable("DB_USER")};" +
                       $"Password={Environment.GetEnvironmentVariable("DB_PASS")};";

// Load cấu hình
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Add controllers
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Swagger + JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo định dạng: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});


// DbContext
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
//Map Enum
dataSourceBuilder.MapEnum<OrderStatus>("order_status");
var dataSource = dataSourceBuilder.Build();
builder.Services.AddDbContext<MyDbContext>(options => options.UseNpgsql(dataSource));

// DI các Repository và Service
// Service
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService,AuthService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped<IS3Service, S3Service>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddSingleton<PayPalClient>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IElasticSearchService, ElasticSearchService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IShipperService, ShipperService>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrdersDetailRepository, OrdersDetailRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductImageService, ProductImageService>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IShipperRepository, ShipperRepository>();

// UnitOfWork
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add mapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));


// Authentication + xử lý lỗi không có token
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };

        // ✅ Xử lý lỗi xác thực token (không có hoặc sai)
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    "{\"status\":401,\"message\":\"Unauthorized: Invalid token\"}"
                );
            },
            OnChallenge = context =>
            {
                context.HandleResponse(); // Ngăn lỗi mặc định
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                return context.Response.WriteAsync(
                    "{\"status\":401,\"message\":\"Unauthorized: Token is missing or expired\"}"
                );
            }
        };
    });

builder.Services.AddAuthorization();
// Add service CACHING
builder.Services.AddOutputCache();
// Create policy cache
builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("cache-long", p => p.Expire(TimeSpan.FromDays(30)));
});


var redisHost = Environment.GetEnvironmentVariable("REDIS_HOST") ?? "localhost";
var redisPort = Environment.GetEnvironmentVariable("REDIS_PORT") ?? "6379";
var redisPassword = Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? "";
var redisInstance = Environment.GetEnvironmentVariable("REDIS_INSTANCE") ?? "swd392-backend:";

var redisConnectionString = $"{redisHost}:{redisPort},password={redisPassword}";

// Cấu hình cache Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = redisInstance;
});

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = new ConfigurationOptions
    {
        EndPoints = { $"{redisHost}:{redisPort}" },
        AbortOnConnectFail = false, // 👈 THÊM DÒNG NÀY!
        ConnectRetry = 5,           // Retry 5 lần
        ConnectTimeout = 5000       // Timeout sau 5s
    };

    if (!string.IsNullOrWhiteSpace(redisPassword))
    {
        config.Password = redisPassword;
    }

    return ConnectionMultiplexer.Connect(config);
});

var app = builder.Build();
app.MapGet("/set-cache", async (IDistributedCache cache) =>
{
    await cache.SetStringAsync("testKey", "Redis OK!", new DistributedCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    });

    return Results.Ok("Cache set");
});

app.MapGet("/get-cache", async (IDistributedCache cache) =>
{
    var value = await cache.GetStringAsync("testKey");
    return Results.Ok(value ?? "Cache not found");
});

// Middleware pipeline
app.UseOutputCache();
app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseCors("CorsPolicy");

app.UseAuthentication(); // ✅ Trước Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();