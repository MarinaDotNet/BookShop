using Asp.Versioning;
using BookShop.API.Controllers.Services;
using BookShop.API.Models;
using BookShop.API.Models.Authentication;
using Microsoft.OpenApi.Models;
using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ProductDatabaseSettings>(builder.Configuration.GetSection("ConnectionMongoStock"));
builder.Services.AddSingleton<StockDBServices>();

//Adding services for Api Middleware
builder.Services.AddTransient<IApiKeyValidator, ApiKeyValidator>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicyForAdmin", policy =>
    {
        policy
        .WithHeaders([ApiConstants.ApiVersionHeader + " : 1"])
        .AllowAnyMethod()
        .DisallowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(30));
    });
    options.AddPolicy("MyPolicyForUser", policy =>
    {
        policy
        .WithHeaders([ApiConstants.ApiVersionHeader + " : 2"])
        .AllowAnyMethod()
        .DisallowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(30));
    });
    options.AddPolicy("MyPolicyForGuest", policy =>
    {
        policy
        .WithHeaders([ApiConstants.ApiVersionHeader + " : 3"])
        .WithMethods("GET", "OPTIONS")
        .DisallowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(30));
    });
});
builder.Services
    .AddApiVersioning(options =>
    {
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = false;
        options.ApiVersionReader = new HeaderApiVersionReader(ApiConstants.ApiVersionHeader);
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddControllers();

//Entity Framework
builder.Services.AddDbContext<AuthenticationApiDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ConnectionToSQL"), builder =>
    builder.EnableRetryOnFailure(maxRetryCount: 10, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null)));

//Add  Identity
builder.Services.AddIdentity<ApiUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthenticationApiDbContext>()
    .AddDefaultTokenProviders();

//Add Authentication
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!))
        };
    });


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    //adding api key scheme
    options.AddSecurityDefinition(ApiConstants.ApiKeyName, new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Description = "Please enter valid Api-Key",
        Name = ApiConstants.ApiKeyHeader,
        Type = SecuritySchemeType.ApiKey
    });

    //adding api key into global security requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = ApiConstants.ApiKeyName
                }
            },
            Array.Empty<string>()
        }
    });

    //adding JWT tokent scheme
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        In = ParameterLocation.Header,
        Description = "Please enter valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    //adding Bearer into global security requirement
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
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

//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

//ensuring that database created
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AuthenticationApiDbContext>();
    context.Database.EnsureCreated();
}
app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ApiMiddleware>();

app.MapControllers();

app.Run();
