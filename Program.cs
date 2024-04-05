using Dapper;
using Margo_Test.Domain;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using System.Reflection.Metadata;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IEmployeeRepository, EmployeeRepository>(provider => {
    var config = provider.GetRequiredService<IConfiguration>();
    var constring = config.GetConnectionString("db") ?? throw new ApplicationException("no sql con string");
    return new EmployeeRepository(constring);
    });

var app = builder.Build();

using (MySqlConnection myConnection = new MySqlConnection(app.Services.GetRequiredService<IConfiguration>().GetConnectionString("db")))
{
    myConnection.Query("CREATE TABLE IF NOT EXISTS Departments (ID INT PRIMARY KEY NOT NULL AUTO_INCREMENT, Name VARCHAR(15) NOT NULL, Phone VARCHAR(11) NOT NULL);");
    myConnection.Query("CREATE TABLE IF NOT EXISTS Employees (ID INT PRIMARY KEY NOT NULL AUTO_INCREMENT, Name VARCHAR(15) NOT NULL, Surname VARCHAR(30) NOT NULL, Phone VARCHAR(11) NOT NULL, CompanyID INT NOT NULL, DepartmentID INT NOT NULL, FOREIGN KEY (DepartmentID) REFERENCES Departments(ID));");
    myConnection.Query("CREATE TABLE IF NOT EXISTS Passports (ID INT PRIMARY KEY NOT NULL AUTO_INCREMENT, Type VARCHAR(15) NOT NULL, Number VARCHAR(30) NOT NULL, EmployeeID INT NOT NULL, FOREIGN KEY (EmployeeID) REFERENCES Employees(ID) ON DELETE CASCADE ON UPDATE CASCADE);");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
