using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.API.Data;
using CityInfo.API.Models;
using CityInfo.API.Services;
using CityInfo.API.ViewModels;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CityInfo.API.Controllers
{
    [Route("api/[controller]")]
    public class CitiesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ICityInfoRepository _cityInfoRepository;

        public CitiesController(IMapper mapper,ICityInfoRepository cityInfoRepository)
        {
            this._mapper = mapper?? 
                throw new ArgumentNullException(nameof(mapper));
            this._cityInfoRepository = cityInfoRepository?? 
                throw new ArgumentNullException(nameof(cityInfoRepository));
        }
        // GET: api/<controller>
        [HttpGet]
        public IActionResult GetCities()
        {
            var cities = _cityInfoRepository.GetCities();
            if (cities!=null)
            {
                return Ok(_mapper.Map<IEnumerable<CityWithoutPointOfInterest>>(cities));
            }

            return NotFound();
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public IActionResult GetCityById(int id,bool includePointOfInterest=false)
        {
            var cityInfo = _cityInfoRepository.GetCity(id, includePointOfInterest);
            if (cityInfo==null)
            {
                return NotFound();
            }
            if (includePointOfInterest)
            {                
                return Ok(_mapper.Map<CityDto>(cityInfo));
            }
           
            return Ok(_mapper.Map<CityWithoutPointOfInterest>(cityInfo));
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
