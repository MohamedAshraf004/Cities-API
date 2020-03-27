using AutoMapper;
using CityInfo.API.Enities;
using CityInfo.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Profiles
{
    public class PointOfInterestProfile:Profile
    {
        public PointOfInterestProfile()
        {
            this.CreateMap<PointOfInterest, PointOfInterestDto>().ReverseMap();
        }
    }
}
