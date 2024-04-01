using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using ODataASPEight.Database;
using ODataASPEight.Model;

//The OData service uses an abstract data model called Entity Data Model
//(EDM) to describe the exposed data in the service.
//The ODataConventionModelBuilder class creates an EDM by using a set of
//default naming conventions EDM, an approach that requires the least code.
//We can use the ODataModelBuilder class to create the EDM if we want more
//control over the EDM.
static IEdmModel GetEdmModel()
{
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<Company>("Companies");
    return builder.GetEdmModel();
}

var builder = WebApplication.CreateBuilder(args);

//The AddOData() method uses the GetEdmModel()method that returns the
//data model, which is the basis of an OData service
builder.Services.AddControllers()
    .AddOData(options => options
    .AddRouteComponents("odata", GetEdmModel())
    .Select()
    .Filter()
    .OrderBy()
    .SetMaxTop(20)
    .Count()
    .Expand()
    );

builder.Services.AddDbContext<ApiContext>(opt => 
opt.UseInMemoryDatabase(databaseName: "CompaniesDB"));

builder.Services.AddScoped<ICompanyRepo, CompanyRepo>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

DBSeeder.AddCompaniesData(app);

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
