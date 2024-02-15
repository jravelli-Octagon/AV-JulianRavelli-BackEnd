using AmericanVirtual.Weather.Challenge.Common.DTOs;
using AmericanVirtual.Weather.Challenge.CoreAPI.Exceptions;
using AmericanVirtual.Weather.Challenge.CoreAPI.Interfaces;
using AmericanVirtual.Weather.Challenge.Repository.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;

namespace AmericanVirtual.Weather.Challenge.CoreAPI.Services
{

    public class WeatherService : IWeatherService
    {
        private readonly IUnitOfWork _UOW;
        private readonly IConfiguration _configuration;

        public WeatherService(IUnitOfWork uow, IConfiguration configuration)
        {
            _UOW = uow ?? throw new ArgumentNullException(nameof(uow));
            _configuration = configuration;
        }

        public async Task<ResponseResultDTO> GetWheatherBy(string province)
        {
            try
            {
                APIResponseDTO result = new APIResponseDTO();
                var client = new RestClient(_configuration.GetSection("MeteoSource:BaseURL").Value + _configuration.GetSection("MeteoSource:Endpoints:Weather").Value);
                
                var request = new RestRequest();
                request.Method = Method.Get;
                request.AddParameter("place_id", province, ParameterType.QueryString);
                request.AddParameter("sections", _configuration.GetSection("MeteoSource:Sections").Value, ParameterType.QueryString);
                request.AddParameter("units", "metric", ParameterType.QueryString);
                request.AddParameter("key", _configuration.GetSection("MeteoSource:ApiKey").Value, ParameterType.QueryString);

                RestResponse response;
                response = await client.ExecuteAsync(request).ConfigureAwait(false);
                
                if(!response.IsSuccessStatusCode)
                {
                   throw new ValidationException(response.ErrorMessage + " - " + response.Content);
                }

                result = JsonConvert.DeserializeObject<APIResponseDTO>(response.Content);
                return ResponseResultDTO.Ok(result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}