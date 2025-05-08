using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using API.Models;
using API.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using API.Models;

using API.Service;
using System.Security.Claims;
using Hangfire;
using Hangfire.MySql;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<OneSignal>(builder.Configuration.GetSection("OneSignal"));
builder.Services.AddScoped<EmailSender>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();

string mySqlConnectionStr = builder.Configuration["ConnectionStrings:MySqlConnection"];

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(mySqlConnectionStr,
       ServerVersion.AutoDetect(mySqlConnectionStr)));

builder.Services.AddIdentity<TaiKhoan, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
            //ClockSkew = TimeSpan.Zero
        };
    });

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("admin", policy =>
//                  policy.RequireClaim(ClaimTypes.Role, "admin"));
//});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API",
        Version = "v1",
        Contact = new OpenApiContact
        {
            Name = "Khue",
            Email = "khuehn155@gmail.com",
        },
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\nExample: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[]{}
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<ThongBaoTuDongService>();
builder.Services.AddHttpClient<OneSignalService>();

builder.Services.AddHangfire(config =>
{
    config.UseStorage(new MySqlStorage(
         mySqlConnectionStr,
         new MySqlStorageOptions
         {
             PrepareSchemaIfNecessary = true,
             TablesPrefix = "Hangfire", 

         }));
});
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 5;
});

var app = builder.Build();

//app.UseHttpsRedirection();
//app.UseRouting();

//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    await SeedData.Initialize(services);
//}

app.UseMiddleware<RequestSizeMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        c.RoutePrefix = "";
    });

}

app.UseHangfireDashboard("/hangfire");
//app.UseHangfireServer();

RecurringJob.AddOrUpdate<ThongBaoTuDongService>(
    "nhac-nho-hien-mau",
    service => service.NhacNhoHienMau(),
    Cron.Daily(23, 30)//+0
);
RecurringJob.AddOrUpdate<ThongBaoTuDongService>(
    "chuc-mung-sinh-nhat",
    service => service.ChucMungSinhNhat(),
    Cron.Daily(23, 30)//+0
);

app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
