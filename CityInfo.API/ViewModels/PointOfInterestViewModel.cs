using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.ViewModels
{
    public class PointOfInterestViewModel
    {
        [Required]
        [MaxLength(50,ErrorMessage ="Name is very long")]
        public string Name { get; set; }

        [MaxLength(200,ErrorMessage ="Descreption is very long")]
        [Required]
        public string Description { get; set; }

    }
}
