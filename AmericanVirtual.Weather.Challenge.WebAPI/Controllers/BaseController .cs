using AmericanVirtual.Weather.Challenge.Common.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AmericanVirtual.Weather.Challenge.WebAPI.Controllers
{
    public abstract class Controller : ControllerBase
    {
        protected ILogger<Controller> _logger;

        protected Controller(ILogger<Controller> logger)
        {
            _logger = logger;
        }

        protected ObjectResult GetObjectResult(ResponseResultDTO result, string approvalResponseMessage = null)
        {
            LogResult();

            if (result.Failed)
            {
                if (result.Code == "403")
                {
                    // return StatusCode(403); // VER COMO DEVOLVER 403
                }

                return BadRequest(result.GetResult);


            }

            return Ok(new
            {
                description = string.IsNullOrEmpty(approvalResponseMessage) ? "La operación se realizó exitosamente." : approvalResponseMessage,
                responseData = result.GetResult
            });
        }

        protected ObjectResult GetErrorObjectResult(Exception ex)
        {
            _logger.LogError("Error en controller: {message} - {stacktrace}", ex.Message, ex.StackTrace);

            return StatusCode((int)HttpStatusCode.InternalServerError, new
            {
                message = "UNEXPECTED_ERROR",
                description = ex.Message
            });
        }

        protected void LogResult()
        {
            _logger.LogInformation("User: {user}. Response: {response}. Trace Identifier: {traceIdentifier}. Session: {session}. Features: {features}. Request: {request}. Connection: {connection}",
                HttpContext.User.ToString(),
                HttpContext.Response.ToString(),
                HttpContext.TraceIdentifier,
                HttpContext.Session.ToString(),
                HttpContext.Features.ToString(),
                HttpContext.Request.ToString(),
                HttpContext.Connection.ToString());
        }
    }
}