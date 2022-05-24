using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private readonly CityInfoContext _context;

        public CityInfoRepository(CityInfoContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // was removed once we added the search/filter/paging version of the same method
        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            // ToListAsync is from Microsoft.EntityFrameworkCore
            return await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        }

        // returning a Tuple of IEnuerable<City>, PaginationMetaData
        // so we get a list of cities along with a single MetaData connected to the list returned
        public async Task<(IEnumerable<City>, PaginationMetaData)> GetCitiesAsync(string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            // collection to start from (case DbSet to IQueryable so we can add filters as needed)
            var collection = _context.Cities as IQueryable<City>;

            if (!string.IsNullOrEmpty(name))
            {
                name = name.Trim();
                collection = collection.Where(c => c.Name == name);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                collection = collection.Where(a => a.Name.Contains(searchQuery)
                    || (a.Description != null && a.Description.Contains(searchQuery)));
            }

            var totalItemCount = await collection.CountAsync();

            var paginationMetaData= new PaginationMetaData(totalItemCount, pageSize, pageNumber);

            var collectionToReturn = await collection.OrderBy(c => c.Name)
                .Skip(pageSize * (pageNumber - 1))  // skip # of records, ex. pageSize = 2, we skip first 10 from page 1
                .Take(pageSize) // take the requested pageSize records, ex. get 10 records
                .ToListAsync();

            return (collectionToReturn, paginationMetaData);
        }

        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {
            if (includePointsOfInterest)
            {
                return await _context.Cities.Include(c => c.PointsOfInterest)
                    .Where(c => c.Id == cityId).FirstOrDefaultAsync();
            }

            return await _context.Cities.Where(c => c.Id == cityId).FirstOrDefaultAsync();
        }

        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId)
        {
            return await _context.PointOfInterest
                .Where(p => p.CityId == cityId && p.Id == pointOfInterestId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId)
        {
            return await _context.PointOfInterest
                .Where(p => p.CityId == cityId).ToListAsync();
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId);
        }

        // this was made asycn Task because we called GetCityAsync, otherwise could be void like Delete
        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            
            if (city != null)
            {
                city.PointsOfInterest.Add(pointOfInterest);
            }
        }
        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.PointOfInterest.Remove(pointOfInterest);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() >= 0;
        }
    }
}
