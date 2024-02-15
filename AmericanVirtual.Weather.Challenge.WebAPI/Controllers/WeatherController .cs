using AmericanVirtual.Weather.Challenge.Common.DTOs;
using AmericanVirtual.Weather.Challenge.CoreAPI.Authorization;
using AmericanVirtual.Weather.Challenge.CoreAPI.Interfaces;
using AmericanVirtual.Weather.Challenge.WebAPI.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmericanVirtual.Weather.Challenge.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : Controller

    {
        private IWeatherService _weatherService;

        public WeatherController(ILogger<WeatherController> logger, IWeatherService weatherService) : base(logger)
        {
            _weatherService = weatherService;
        }

        [AllowAnonymous]
        [HttpGet("getBy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [PermissionAuthorize(PermissionNames.WEATHER)]
        public async Task<IActionResult> GetWheatherBy(string province)
        {
            try
            {
                return GetObjectResult(await _weatherService.GetWheatherBy(province));
            }
            catch (Exception ex)
            {
                return GetErrorObjectResult(ex);
            }
        }

    }
}
