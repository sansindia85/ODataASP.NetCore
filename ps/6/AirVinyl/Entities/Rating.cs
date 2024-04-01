﻿using System.ComponentModel.DataAnnotations;

namespace AirVinyl.Entities
{
    public class Rating
    {
        [Key]
        public int RatingId { get; set; }

        [Required]
        public int Value { get; set; }

        [Required]
        public Person RatedBy { get; set; }
       
        public int RatedByPersonId { get; set; }

        [Required]
        public int RecordStoreId { get; set; }
    }
}
