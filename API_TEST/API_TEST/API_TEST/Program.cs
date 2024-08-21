using API_TEST.DB_Context;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

    
    
builder.Services.AddTransient<DB_Connection>();

builder.Services.AddSwaggerGen(c =>
{

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TEST API", Version = "v1" });
  



    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    //c.IncludeXmlComments(xmlPath);

   

    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
            $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));


});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();
