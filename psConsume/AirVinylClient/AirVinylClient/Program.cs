using AirVinylClient;
using System;

var localContext = new MyLocalContainer(new Uri("http://localhost:5000/odata/"));
//var people = localContext.People.Execute();
//var people = localContext.People.Where(p => p.PersonId == 1);

//Scalar query
var kevin = localContext.People.Where(p => p.PersonId == 1).First();
Console.WriteLine($"{kevin.FirstName} {kevin.LastName}");

//foreach (var person in people)
//{
//    Console.WriteLine($"{person.PersonId} {person.FirstName} {person.LastName}");
//}

var personToCreate = new Person()
{
    FirstName = "Servilla",
    LastName = "Smith",
    AmountOfCashToSpend = 400,
    DateOfBirth = new DateTimeOffset(new DateTime(1980, 5, 10)),
    Email = "someaddress@someserver.com",
    NumberOfRecordsOnWishList = 10
};

localContext.AddPerson(personToCreate);

LogTrackedPeople(localContext);

localContext.SaveChanges();

LogTrackedPeople(localContext);

personToCreate.FirstName = "Marcus";

localContext.UpdateObject(personToCreate);

localContext.SaveChanges();

LogTrackedPeople(localContext);

localContext.DeleteObject(personToCreate);

LogTrackedPeople(localContext);

var people = localContext.People.Execute();

foreach (var person in people)
{
    Console.WriteLine($"{person.PersonId} {person.FirstName} {person.LastName}");
}

void LogTrackedPeople(MyLocalContainer context)
{
    foreach (var entityDescriptor in context.EntityTracker.Entities)
    {
        if (entityDescriptor.Entity is Person castedPerson)
        {
            Console.WriteLine($" {entityDescriptor.State} - {castedPerson.PersonId} +" +
                               $" {castedPerson.FirstName} {castedPerson.LastName}");
        }
    }
}

Console.ReadLine();

