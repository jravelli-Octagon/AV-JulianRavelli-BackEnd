using AmericanVirtual.Weather.Challenge.Common.DTOs;

namespace AmericanVirtual.Weather.Challenge.CoreAPI.Interfaces
{
    public interface IWeatherService
    {
        Task<ResponseResultDTO> GetWheatherBy(string province);
    }
}