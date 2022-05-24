using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
    [Authorize]
    public class CitiesController : ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;
        const int maxCitiesPageSize = 20;   // to prevent the user from passing in 1000 in url for GetCities call as pageSize

        // IMapper from automapper, we already registered AutoMapper in Program, don't have to do IMapper separately in Program.cs
        public CitiesController(ICityInfoRepository cityInfoRepository, IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // .net will know name is from querystring, but we could have also added FromQuery for readability.
        // if it's name differently in url from param name then use Name property like [FromQuery(Name = "filteronname")]string? name
        // will map query string "filteronname" to method parameter "name"
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities(string? name, string? searchQuery, 
            int pageNumber = 1, int pageSize = 10)
        {
            // if user passes larger then 20, force it to 20 to prevent performance issues user may cause by playing with url
            if (pageSize > maxCitiesPageSize)
            {
                pageSize = maxCitiesPageSize;
            }

            // getting a Tuple (list of cities and a metadata object)
            var (cityEntities, paginationMetaData) = await _cityInfoRepository.GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

            // add pagination metadata to header of response
            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetaData));
            
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
