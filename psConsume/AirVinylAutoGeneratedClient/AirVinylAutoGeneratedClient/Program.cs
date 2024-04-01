// See https://aka.ms/new-console-template for more information
using AirVinyl;
using Microsoft.OData.Client;

var airVinylContainer = new AirVinylContainer(new Uri("https://localhost:5001/odata/"));

//var people = await airVinylContainer.People.ExecuteAsync();
//var recordStores = await airVinylContainer.RecordStores.ExecuteAsync();

//var peopleQuery = airVinylContainer.People;
var peopleQuery = airVinylContainer.People.Where(p => p.PersonId == 1) as DataServiceQuery;
var recordStoresQuery = airVinylContainer.RecordStores;

var batchResponse = await airVinylContainer.ExecuteBatchAsync(
       peopleQuery,
       recordStoresQuery);

foreach (var operationResponse in batchResponse)
{
    var peopleResponse = operationResponse as QueryOperationResponse<Person>;
    
    if (peopleResponse != null)
    {
        foreach (var person in peopleResponse)
        {
            Console.WriteLine($"{person.PersonId} {person.FirstName} {person.LastName}");
        }
    }

    var recordsStoreResponse = operationResponse as QueryOperationResponse<RecordStore>;

    if (recordsStoreResponse != null)
    {
        foreach (var recordStore in recordsStoreResponse)
        {
            Console.WriteLine($"{recordStore.RecordStoreId} {recordStore.Name}");
        }
    }    
}

Console.WriteLine("------------------");

airVinylContainer.AddToPeople(new AirVinyl.Person
{
    FirstName = "John",
    LastName = "Doe",
    AmountOfCashToSpend = 400,
    DateOfBirth = new DateTimeOffset(new DateTime(1980, 5, 10)),
    Email = "someaddress@someserver.com",
    NumberOfRecordsOnWishList = 10,
    Gender = Gender.Male
});


airVinylContainer.AddToPeople(new AirVinyl.Person
{
    FirstName = "John the second",
    LastName = "Doe",
    AmountOfCashToSpend = 400,
    DateOfBirth = new DateTimeOffset(new DateTime(1980, 5, 10)),
    Email = "someaddress@someserver.com",
    NumberOfRecordsOnWishList = 10,
    Gender = Gender.Male
});

//SaveChangesOptions.BatchWithSingleChangeset : Will save changes in a single changeset.
//                                              If one request fails,all request will fail
//SaveChangesOptions.BatchWithIndependentOperations : Save each change change independently in the batch request.
//                                              If one request fails, other requests are not affected.
var result = await airVinylContainer.SaveChangesAsync(SaveChangesOptions.BatchWithSingleChangeset);

var people = airVinylContainer.People;

foreach (var person in people)
{
    Console.WriteLine($"{person.PersonId} {person.FirstName} {person.LastName}");
}

Console.ReadLine();

static void CodeGeneration()
{
    var airVinylContainer = new AirVinylContainer(new Uri("https://localhost:5001/odata/"));
    //var people = await airVinylContainer.People
    //    .AddQueryOption("$expand", "VinylRecords")
    //    .AddQueryOption("$select", "PersonId,FirstName")
    //    .AddQueryOption("$orderby", "FirstName")
    //    .AddQueryOption("$top", "2")
    //    .AddQueryOption("$skip", "4")
    //    .ExecuteAsync();

    var people = airVinylContainer.People
        //.Expand(p => p.VinylRecords)
        .OrderBy(p => p.FirstName)
        .Skip(4)
        .Take(2)
        .Select(p => new { p.PersonId, p.FirstName, VinylRecords = p.VinylRecords });

    foreach (var person in people)
    {
        Console.WriteLine($"{person.PersonId} {person.FirstName}");

        //airVinylContainer.LoadProperty(person, "VinylRecords");

        foreach (var vinylRecord in person.VinylRecords)
        {
            Console.WriteLine($"----  {vinylRecord.VinylRecordId} {vinylRecord.Title}");
        }
    }

    //var kevin = await airVinylContainer.People.ByKey(1).GetValueAsync();
    //Console.WriteLine($"{kevin.PersonId} {kevin.FirstName} {kevin.LastName}");
}