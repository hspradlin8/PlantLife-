using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Plant_Life.Models
{
    public class Plant
    {
        public ApplicationUser ApplicationUser { get; set; }
        public string ApplicationUserId { get; set; }

        public int Id { get; set; }
        [Required]
        [Display(Name = "Plant Name")]
        public string PlantName { get; set; }
        public string Sunlight { get; set; }
        public string Temperature { get; set; }
     [Display(Name = "Water frequency (in days)")]
        public int WaterNeeds { get; set; }
        //public string Description { get; set; }
        public string Issues { get; set; }
        public int Quantity { get; set; }

        [DataType(DataType.Date)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DateCreated { get; set; }
        public string? Image { get; set; }
        [NotMapped]
        public IFormFile File { get; set; }

    }
}
