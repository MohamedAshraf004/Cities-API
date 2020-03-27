using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private readonly ICityInfoRepository _cityInfoRepository;

        private readonly IMapper _mapper ; 

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
                                            IMailService mailService,
                                            ICityInfoRepository cityInfoRepository,
                                            IMapper mapper)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            this._cityInfoRepository = cityInfoRepository?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper?? throw new ArgumentNullException(nameof(mapper));
        }
        // GET api/values
        [HttpGet]
        public IActionResult GetPointsOfInterset(int cityId)
        {
            try
            {
                if (!_cityInfoRepository.CityExists(cityId))
                {
                    logger.LogInformation($"City with id {cityId} wasn't found when " +
                        $"accessing points of interest.");
                    return NotFound();
                }
                var pointsOfInterest = _cityInfoRepository.GetPointsOfInterestForCity(cityId);

                return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterest));
            }
            catch (Exception ex)
            {
                logger.LogCritical($"Exception while getting points of interest for city with id {cityId}.", ex);
                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        // GET api/values/5
        [HttpGet("{id}",Name = "GetPointOFInterest")]
        public IActionResult GetPointOfInterestById(int cityId,int id)
        {
            try
            {
                if (!_cityInfoRepository.CityExists(cityId))
                {
                    logger.LogInformation($"City with id {cityId} wasn't found when " +
                        $"accessing points of interest.");
                    return NotFound();
                }
                var pointOfInterest = _cityInfoRepository.GetPointOfInterestForCity(cityId,id);
                if (pointOfInterest == null)
                {
                    return NotFound();
                }
                return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));
            }
            catch (Exception ex)
            {
                logger.LogCritical($"Exception while getting points of interest for city with id {cityId}.", ex);
                return StatusCode(500, "A problem happened while handling your request.");
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
