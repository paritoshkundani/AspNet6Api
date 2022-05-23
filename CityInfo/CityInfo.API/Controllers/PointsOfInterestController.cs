using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly CitiesDataStore _citiesDataStore;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, IMailService mailService, CitiesDataStore citiesDataStore)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService));
            _citiesDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
            /*
            * if for any reason you want to manually request a service:
            * HttpContext.RequestServices.GetService
            * but it's always better to use dependency injection to get it instead or manually
            */
        }

        [HttpGet]
        public ActionResult<IEnumerable<PointOfInterestDto>> GetPointsOfInterest(int cityId)
        {
            try
            {
               // to see logging of exception, undo the below ([FTL] tag in Serilog file/console)
               // throw new Exception("Exception sample");

                // city to find
                var city = _citiesDataStore.Cities
                    .FirstOrDefault(c => c.Id == cityId);

                if (city == null)
                {
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest");
                    return NotFound();
                }

                return Ok(city.PointsOfInterest);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}", ex);
                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public ActionResult<PointOfInterestDto> GetPointOfInterest(int cityId, int pointOfInterestId)
        {
            // city to find
            var city = _citiesDataStore.Cities
                .FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointsOfInterest
                .FirstOrDefault(x => x.Id == pointOfInterestId);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            return Ok(pointOfInterestFromStore);
        }

        [HttpPost]
        // API will assume FromBody is there for pointOfInterest as it's a complex type, so that attribute is optional
        public ActionResult<PointOfInterestDto> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
        {
            // city to find
            var city = _citiesDataStore.Cities
                .FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            // demo puroses - to be improved
            var maxPointOfInterestId = _citiesDataStore.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

            var finalPointOfInterest = new PointOfInterestDto()
            {
                Id = ++maxPointOfInterestId,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            city.PointsOfInterest.Add(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    pointOfInterestId = finalPointOfInterest.Id
                },
                finalPointOfInterest
            );
        }

        [HttpPut("{pointofinterestid}")]
        public ActionResult UpdatePointOfInterest(int cityId, int pointOfInterestId, PointOfInterestForUpdateDto pointOfInterest)
        {
            // city to find
            var city = _citiesDataStore.Cities
                .FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointsOfInterest
                .FirstOrDefault(x => x.Id == pointOfInterestId);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            pointOfInterestFromStore.Name = pointOfInterest.Name;
            pointOfInterestFromStore.Description = pointOfInterest.Description;

            // can return Ok as well, but since the user already has the update model (it was sent in), no
            // need to return the same model that was sent it, traditionally NoContent is returned for PUT
            return NoContent();
        }

        [HttpPatch("{pointofinterestid}")]
        public ActionResult PartiallyUpdatePointOfInterest(int cityId, int pointOfInterestId, JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            // city to find
            var city = _citiesDataStore.Cities
                .FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointsOfInterest
                .FirstOrDefault(x => x.Id == pointOfInterestId);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            // we need a PointOfInterestForUpdateDto which we will patch, so get data from PointOfInterestDto object
            var pointOfInterestToPatch =
                new PointOfInterestForUpdateDto()
                {
                    Name = pointOfInterestFromStore.Name,
                    Description = pointOfInterestFromStore.Description
                };

            // apply the patch, pass in ModelState to have the validation checks done on the jsonpatch document
            // to confirm jsonpatch is in valid format, this does not check if Dto is valid
            patchDocument.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // even if jsonpatch document is valid, does not mean the model is valid, ex. remove the name property
            // if jsonpatch has /remove on name, jsonpatch is valid format, but that should mark the Dto as invalid
            // as name is required, so we need to check the Dto also after applying patch to it
            if (!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            // if all valid apply the updates to the PointOfInterestDto object
            pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
            pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public ActionResult DeletePointOfInterest(int cityId, int pointOfInterestId)
        {
            // city to find
            var city = _citiesDataStore.Cities
                .FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointsOfInterest
                .FirstOrDefault(x => x.Id == pointOfInterestId);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            city.PointsOfInterest.Remove(pointOfInterestFromStore);

            _mailService.Send("Point of interest deleted",
                $"Point of interest {pointOfInterestFromStore.Name} with id {pointOfInterestFromStore.Id} was deleted");

            return NoContent();
        }
    }
}
