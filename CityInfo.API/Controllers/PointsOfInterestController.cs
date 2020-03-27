using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.API.Data;
using CityInfo.API.Enities;
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
            
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }
            var pointOfInterest = _mapper.Map<PointOfInterest>(model);
            _cityInfoRepository.AddPointOfInterestForCity(cityId, pointOfInterest);
            _cityInfoRepository.Save();
            var createdPointOfInterestToReturn = _mapper.Map<PointOfInterestDto>(pointOfInterest);
            
            return CreatedAtRoute("GetPointOFInterest",
                            new { cityId,id= createdPointOfInterestToReturn.Id},
                            createdPointOfInterestToReturn);
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
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound("City not found");
            }
            var oldPointOfInterest = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (oldPointOfInterest==null)
            {
                return NotFound("Point of interest not found");
            }

            _mapper.Map(model, oldPointOfInterest);
            _cityInfoRepository.Save();

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
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound("City not found");
            }
            var oldPointOfInterest = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (oldPointOfInterest == null)
            {
                return NotFound("Point of interest not found");
            }
            var pointOfInterestTOPatch = 
                    _mapper.Map<PointOfInterestViewModel>(oldPointOfInterest);
            patchDoc.ApplyTo(pointOfInterestTOPatch, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (pointOfInterestTOPatch.Description == pointOfInterestTOPatch.Name)
            {
                ModelState.AddModelError(
                    "Description",
                    "The provided description should be different from the name.");
            }

            if (! TryValidateModel(pointOfInterestTOPatch))
            {
                return BadRequest(ModelState);
            }
            _mapper.Map(pointOfInterestTOPatch, oldPointOfInterest);

            _cityInfoRepository.UpdatePointOfInterestForCity(cityId, oldPointOfInterest);

            _cityInfoRepository.Save();

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
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound("City not found");
            }
            var deletedPointOfInterest = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (deletedPointOfInterest == null)
            {
                return NotFound("Point of interest not found");
            }

            _cityInfoRepository.DeletePointOfInterest(deletedPointOfInterest);

            _cityInfoRepository.Save();
            _mailService.Send("Point of interest was deleted"
                    ,$"Point of interest { deletedPointOfInterest.Name} with id { deletedPointOfInterest.Id} was deleted.");
            return NoContent();
        }
    }
}
