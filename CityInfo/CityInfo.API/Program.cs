using CityInfo.API;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Serilog;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

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
