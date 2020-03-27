using System.ComponentModel.DataAnnotations;

namespace CityInfo.API.ViewModels
{
    public class PointOfInterestViewModel
    {
        [Required]
        [MaxLength(50, ErrorMessage = "Name is very long")]
        public string Name { get; set; }

        [MaxLength(200, ErrorMessage = "Descreption is very long")]
        [Required]
        public string Description { get; set; }

    }
}
