using CityInfo.API.Models;
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
        private readonly CitiesDataStore _citiesDataStore;

        public CitiesController(CitiesDataStore citiesDataStore)
        {
            _citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
        }

        [HttpGet]
        public ActionResult<IEnumerable<CityDto>> GetCities()
        {
            return Ok(_citiesDataStore.Cities);
        }

        //[HttpGet("{id}")]
        //public JsonResult GetCity(int id)
        //{
        //    return new JsonResult(CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id));
        //}

        [HttpGet("{id}")]
        public ActionResult<CityDto> GetCity(int id)
        {
            // since we are not directly returning Json (as above commented out version)
            // we are using Ok() that returns an ActionResult<T> rather than JsonResult, ActionResult
            // will allow JSON/XML/etc.. whatever the API is built to support to be returned if requested (accept header)

            // city to find
            var cityToReturn = _citiesDataStore.Cities
                .FirstOrDefault(c => c.Id == id);
            
            if (cityToReturn == null)
            {
                return NotFound();
            }

            return Ok(cityToReturn);
        }

    }
}
