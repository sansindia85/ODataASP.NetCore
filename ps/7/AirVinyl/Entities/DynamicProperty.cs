using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace AirVinyl.Entities
{
    [Table("DynamicVinyRecordProperties")]
    public class DynamicProperty
    {
        public string Key { get; set; }
        public string SerializedValue { get; set; }

        //EF Core can't store obbject values, so we need to work
        //with an in-between property to/from JSON representation
        [NotMapped]
        public object Value
        {
            get
            {
                return JsonConvert.DeserializeObject(SerializedValue);
            }
            set
            {
                SerializedValue = JsonConvert.SerializeObject(value);
            }
        }

        public int VinylRecordId { get; set; }
        public virtual VinylRecord VinylRecord { get; set; }
    }

}
