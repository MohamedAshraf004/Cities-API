using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Data;
using CityInfo.API.Models;
using CityInfo.API.Services;
using CityInfo.API.ViewModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CityInfo.API.Controllers
{
    [Route("api/Cities/{cityId}/[controller]")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> logger;
        private readonly IMailService _mailService;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
                                            IMailService mailService)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
        }
        // GET api/values
        [HttpGet]
        public IActionResult GetPointsOfInterset(int cityId)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }
            return Ok(city.PointsOfInterest);
        }

        // GET api/values/5
        [HttpGet("{id}",Name = "GetPointOFInterest")]
        public IActionResult GetPointOfInterestById(int cityId,int id)
        {
            try
            {
                //throw new Exception("Try Exception");
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                logger.LogInformation($"City with id {cityId}  was not found ");

                return NotFound();
            }
            var pointOfInterest = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if (pointOfInterest==null)
            {
                return NotFound();
            }
            return Ok(pointOfInterest);

            }
            catch(Exception ex)
            {
                logger.LogCritical($"Exception while getting point of interest ", ex);
                return StatusCode(500, "Error while handling the request");
            }
        }

        
        [HttpPost]
        public IActionResult CreatePointOfInterest(int cityId,[FromBody] PointOfInterestViewModel model)
        {
            if (model.Description==model.Name)
            {
                ModelState.AddModelError("Description", "Description must be different from the name");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }
            var maxId = CitiesDataStore.Current.Cities.SelectMany(p => p.PointsOfInterest).Max(d => d.Id);
            PointOfInterestDto pointOfInterest = new PointOfInterestDto
            {
                Id = ++maxId,
                Name = model.Name,
                Description = model.Description
            };
            city.PointsOfInterest.Add(pointOfInterest);
            return CreatedAtRoute("GetPointOFInterest",new { cityId,id=pointOfInterest.Id},pointOfInterest);

        }

        // PUT api/cities/cityId/point/5
        [HttpPut("{id}")]
        public IActionResult UpdatePointOfInterest(int cityId,int id, [FromBody] PointOfInterestViewModel model)
        {
            if (model.Description == model.Name)
            {
                ModelState.AddModelError("Description", "Description must be different from the name");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }
            var pointOfInterest = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if (pointOfInterest == null)
            {
                return NotFound();
            }
            pointOfInterest.Description = model.Description;
            pointOfInterest.Name = model.Name;

            return NoContent();
        }
        [HttpPatch("{id}")]
        public IActionResult PartialUpdatePointOfInterest(int cityId, int id,
                            [FromBody] JsonPatchDocument<PointOfInterestViewModel> patchDoc)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }
            var pointOfInterestFromStore = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }
            var pointOfInterestTOPatch = new PointOfInterestViewModel
            {
                Description = pointOfInterestFromStore.Description,
                Name = pointOfInterestFromStore.Name
            };
            patchDoc.ApplyTo(pointOfInterestTOPatch, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (! TryValidateModel(pointOfInterestTOPatch))
            {
                return BadRequest(ModelState);
            }
            pointOfInterestFromStore.Name = pointOfInterestTOPatch.Name;
            pointOfInterestFromStore.Description = pointOfInterestTOPatch.Description;
            return NoContent();
            /*
             Shape of request
                [
                  {
                    "op": "replace"
                    "path": "/name",
                    "value": "update from patch",
                  }
                ]
             */
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public IActionResult DeletePointOfInterest(int cityId,int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }
            var pointOfInterestToBeDeleted = city.PointsOfInterest.FirstOrDefault(p => p.Id == id);
            if (pointOfInterestToBeDeleted == null)
            {
                return NotFound();
            }
            city.PointsOfInterest.Remove(pointOfInterestToBeDeleted);
            _mailService.Send("Point of interest was deleted"
                    ,$"Point of interest { pointOfInterestToBeDeleted.Name} with id { pointOfInterestToBeDeleted.Id} was deleted.");
            return NoContent();
        }
    }
}
