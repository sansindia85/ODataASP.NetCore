// See https://aka.ms/new-console-template for more information
using BasicCrud.Models;
using System;

Console.WriteLine("Hello, World!");

async Task ListPeople()
{
    var serviceRoot = "https://services.odata.org/V4/TripPinServiceRW/";
    var context = new CustomerSingle()

    IEnumerable<Person> people = await context.People.ExecuteAsync();
    foreach (var person in people)
    {
        Console.WriteLine("{0} {1}", person.FirstName, person.LastName);
    }
}