// See https://aka.ms/new-console-template for more information
using ODataSamples.WebApiService.Models;

Console.WriteLine("Hello, World!");

await ListPeople();
async Task ListPeople()
{
    var serviceRoot = "http://localhost:23890/";
    var context = new DefaultContainer(new Uri(serviceRoot));

    IEnumerable<Person> people = await context.People.ExecuteAsync();
    foreach (var person in people)
    {
        Console.WriteLine("{0} {1}", person.FirstName, person.LastName);
    }
}

AddAnEntity();
void AddAnEntity()
{
    var context = new DefaultContainer(new Uri("http://localhost:23890/"));
    var person = Person.CreatePerson("dalip", "negi", "s", PersonGender.Male, 1);
    context.AddToPeople(person);

    context.SaveChanges();
}

await ListPeople();



//Update an entity

var context = new DefaultContainer(new Uri("http://localhost:23890/"));

var person = context.People.ByKey(userName: "dalip").GetValue(); // get an entity
person.FirstName = "dalip"; // change its property
context.UpdateObject(person); // create an update request

context.SaveChanges(); // send the request

//Delete an entity
var context1 = new DefaultContainer(new Uri("http://localhost:23890/"));

var person1 = context1.People.ByKey(userName: "dalip").GetValue(); // get an entity
context1.DeleteObject(person1); // create a delete request

context1.SaveChanges(); // send the request