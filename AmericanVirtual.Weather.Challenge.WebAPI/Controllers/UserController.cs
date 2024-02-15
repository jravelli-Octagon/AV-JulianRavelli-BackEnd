using AmericanVirtual.Weather.Challenge.Common.DTOs;
using AmericanVirtual.Weather.Challenge.CoreAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmericanVirtual.Weather.Challenge.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private IAuthenticationService _authenticationService;

        public UserController(ILogger<UserController> logger, IAuthenticationService userService) : base(logger)
        {
            _authenticationService = userService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Authenticate([FromHeader(Name = "x-password")] string password, [FromHeader(Name = "x-username")] string username, [FromHeader(Name = "x-recaptcha")] string recaptcha)
        {
            try
            {
                UserDTO userDTO = new UserDTO()
                {
                    Username = username,
                    Password = password
                };

                return GetObjectResult(await _authenticationService.Authenticate(userDTO, recaptcha));
            }
            catch (Exception ex)
            {
                return GetErrorObjectResult(ex);
            }
        }

        [AllowAnonymous]
        [HttpPost("password/recover")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RecoverPassword([FromQuery] string email, [FromQuery] bool isBillingPassChange = false)
        {
            try
            {
                return GetObjectResult(await _authenticationService.RecoverPassword(email, isBillingPassChange));
            }
            catch (Exception ex)
            {
                return GetErrorObjectResult(ex);
            }
        }

        [AllowAnonymous]
        [HttpPost("password/assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AssignPassword([FromBody] ChangePasswordDTO newPassword, [FromQuery] string hash)
        {
            try
            {
                return GetObjectResult(await _authenticationService.AssignPassword(hash, newPassword.Password));
            }
            catch (Exception ex)
            {
                return GetErrorObjectResult(ex);
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromHeader(Name = "x-token")] string token, [FromHeader(Name = "x-refresh")] string refreshToken)
        {
            try
            {
                return GetObjectResult(await _authenticationService.RefreshToken(token, refreshToken));
            }
            catch (Exception ex)
            {
                return GetErrorObjectResult(ex);
            }
        }

        [AllowAnonymous]
        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] UserDTO userDTO)
        {
            try
            {
                return GetObjectResult(await _authenticationService.Create(userDTO));
            }
            catch (Exception ex)
            {
                return GetErrorObjectResult(ex);
            }
        }
    }
}
