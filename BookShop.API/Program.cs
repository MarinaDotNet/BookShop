using Asp.Versioning;
using BookShop.API.Controllers.Services;
using BookShop.API.Models;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ProductDatabaseSettings>(builder.Configuration.GetSection("ConnectionMongoStock"));
builder.Services.AddSingleton<StockDBServices>();

//Adding services for Api Middleware
builder.Services.AddTransient<IApiKeyValidator, ApiKeyValidator>();
builder.Services.AddHttpContextAccessor();

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
});

//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSerilogRequestLogging();

app.UseAuthorization();

app.UseMiddleware<ApiMiddleware>();

app.MapControllers();

app.Run();
