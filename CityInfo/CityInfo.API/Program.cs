using CityInfo.API;
using CityInfo.API.DbContexts;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

// Serilog setup (Log and LoggerConfiguration both are from Serilog) afterward will tell .net to use it
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt", rollingInterval: RollingInterval.Day)    // create a file each day
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// logging is built into the CreateBuilder method, it's configured by default to log to
// console/debug window/event source and is setup to look for the Logging section of
// our default settings file, which for us is appsettings.json, for details.
// appsettings are also based on environment, so for us its appsettings.development.json
// Since it's built in we did not have to add it as a service, it's already done
// for us.
// He decided to clear all of the logging and only include the console for now
// after setting up serilog just commented them out as serilog will do console and file logging now
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();

// tell .net to use Serilog
builder.Host.UseSerilog();

// here if we used builder.Services.AddMvc() instead it would work, but we would get
// extra features which we don't need, API will just return JSON, no need for other types
// or views so sticking with AddControllers()
// AddNewtonsoftJson is for jsonpatch support along with 2 nuget packages (jsonpatch and newtonsoftjson)
// other by default we use the Microsoft Json library, which is lighter but does not support json patch
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true; // if user requests anything but json or xml return not acceptable (406), we default to json
}).AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters(); // add support for xml in accept header

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// helps us figure out the content type of a file
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

#if DEBUG
builder.Services.AddTransient<IMailService, LocalMailService>();
#else
builder.Services.AddTransient<IMailService, CloudMailService>();
#endif

builder.Services.AddSingleton<CitiesDataStore>();

// AddDbContext will register with Scoped Lifetime
// using SqlLite, this will make a file called CityInfo.db on the root of the application
// for production settings, using system level environment variable, we name it same "ConnectionStrings:CityInfoDBConnectionString"
// environment variable for us is not a real url, just something he used to show it pulling from there when in production
// NOTE: environment variable will always override anything in any appsettings, even Development, so you can't have one system
// where it will from appsettings for DEV and same machine environment variable for PROD, as it will always trump others
// so we removed it from environment afterwards to allow appsettings to be used.
// NOTE: in order for VS to pick up the newly added environment variable, VS will need to be restarted.  Same when removing it
builder.Services.AddDbContext<CityInfoContext>(
    dbContextOptions => dbContextOptions.UseSqlite(builder.Configuration["ConnectionStrings:CityInfoDBConnectionString"]));

builder.Services.AddScoped<ICityInfoRepository, CityInfoRepository>();

// register AutoMapper - telling it to scan the current assembly for profiles (profile is a nice way to organize automapper mappings)
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// add authentication (default challenge to bearer and then configure it 
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"],
            ValidAudience = builder.Configuration["Authentication:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["Authentication:SecretForKey"]))
        };
    });

// adding a policy, only people who live in Antwerp can access
// policy is made up of a set of requirements, when all are met it's true
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MustBeFromAntwerp", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("city", "Antwerp");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

// add authentication check to the pipeline
app.UseAuthentication();

app.UseAuthorization();

app.UseEndpoints(endPoints =>
{
    endPoints.MapControllers(); // we will use attribute routing
});

/* 
  before had to use
  app.Routing();
  app.UseAuthorization();
  app.UseEndPoints() -> adding MapControllers in here
  but in .Net 6 they do the Routing/EndPoints via MapControllers below
  but he preferred the other approach so used that instead, commenting out MapControllers
  and adding above pieces
*/
// app.MapControllers();   

app.Run();
