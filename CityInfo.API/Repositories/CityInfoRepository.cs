using CityInfo.API.Contexts;
using CityInfo.API.Enities;
using CityInfo.API.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Repositories
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly AppDbContext _dbContext;

        public CityInfoRepository(AppDbContext dbContext)
        {
            this._dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }
        public void AddPointOfInterestForCity(int cityId, PointOfInterest pointOfInterest)
        {
           var city= _dbContext.Cities.Where(c => c.Id == cityId).FirstOrDefault();
            city.PointsOfInterest.Add(pointOfInterest);
        }

        public bool CityExists(int cityId)
        {
            return _dbContext.Cities.Any(c=>c.Id==cityId); ;
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {            
                _dbContext.PointOfInterests.Remove(pointOfInterest);
        }

        public IEnumerable<City> GetCities()
        {
            return _dbContext.Cities.OrderBy(c => c.Name).ToList();
        }

        public City GetCity(int cityId,bool includePointOfInterest=false)
        {
            if (includePointOfInterest)
            {
                return _dbContext.Cities.Include(p => p.PointsOfInterest)
                    .FirstOrDefault(c => c.Id == cityId);
            }
            return _dbContext.Cities.FirstOrDefault(c => c.Id == cityId);
        }

        public PointOfInterest GetPointOfInterestForCity(int cityId, int pointOfInterestId)
        {
            return _dbContext.PointOfInterests
                .FirstOrDefault(p => p.CityId == cityId && p.Id == pointOfInterestId);
        }

        public IEnumerable<PointOfInterest> GetPointsOfInterestForCity(int cityId)
        {
            return _dbContext.PointOfInterests.Where(p => p.CityId == cityId);
        }

        public bool Save()
        {
            return _dbContext.SaveChanges() >=1;
        }

        public void UpdatePointOfInterestForCity(int cityId, PointOfInterest pointOfInterest)
        {
          
        }
    }
}
