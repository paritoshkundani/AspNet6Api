using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// here if we used builder.Services.AddMvc() instead it would work, but we would get
// extra features which we don't need, API will just return JSON, no need for other types
// or views so sticking with AddControllers()
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true; // if user requests anything but json or xml return not acceptable (406), we default to json
}).AddXmlDataContractSerializerFormatters(); // add support for xml in accept header

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// helps us figure out the content type of a file
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

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
