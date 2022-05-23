using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    /*
     -- ControllerBase give us basic API functionality, like accessing user information and common methods for returning responses
     -- Controller could also be used, but that gives adding functionality for returning views, we are not returning views in API
     
    -- ApiController isn't neccessary but good to use, it provides improved development experience when building APIs, like automatically
       returning a 400 error on bad input and returning problem details on errors so try to use it.  It will also automatically
       check for DataAnnotation (ex. Required) on models being passed it, so no need to check ModelState.IsValid all the 
       time with this in place.

     -- Route could have also been [Route("api/[controller]") but he preferred writing it out as if you change the controller name
        that would break existing calls as they will also need changing, here if we rename the controller our route is still /api/cities
    */

    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        // IMapper from automapper, we already registered AutoMapper in Program, dont have to do IMapper separately in Program.cs
        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities()
        {
            var cityEntities = await _cityInfoRepository.GetCitiesAsync();

            var results = new List<CityWithoutPointsOfInterestDto>();
            
            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities));
        }

        //[HttpGet("{id}")]
        //public JsonResult GetCity(int id)
        //{
        //    return new JsonResult(CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id));
        //}

        // below returning IActionResult since we can return 2 different types in same method
        // since he did not include includePointsOfInterest in HttpGet[] we have to use query
        // string ?includePointsOfInterest=true to trigger that
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCity(int id, bool includePointsOfInterest = false)
        {
            // since we are not directly returning Json (as above commented out version)
            // we are using Ok() that returns an ActionResult<T> rather than JsonResult, ActionResult
            // will allow JSON/XML/etc.. whatever the API is built to support to be returned if requested (accept header)

            var city = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);
            
            if (city == null)
            {
                return NotFound();
            }

            if (includePointsOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(city));
            }

            return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(city));
        }

    }
}
