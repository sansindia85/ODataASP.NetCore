using AirVinyl.Entities;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace AirVinyl.EntityDataModels
{
    public class AirVinylEntityDataModel
    {
        public IEdmModel GetEntityDataModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.Namespace = "AirVinyl";
            builder.ContainerName = "AirVinylContainer";

            builder.EntitySet<Person>("People");
            builder.EntitySet<VinylRecord>("VinylRecords");
            
            return builder.GetEdmModel();
        }
    }
}
