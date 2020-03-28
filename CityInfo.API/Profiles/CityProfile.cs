using AutoMapper;
using CityInfo.API.Enities;
using CityInfo.API.Models;
using CityInfo.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Profiles
{
    public class CityProfile :Profile
    {
        public CityProfile()
        {
            this.CreateMap<City,CityDto>().ReverseMap();
            this.CreateMap<City,CityWithoutPointOfInterest>().ReverseMap();
            this.CreateMap<City,CityCreatedViewModel>().ReverseMap();
        }
    }
}
