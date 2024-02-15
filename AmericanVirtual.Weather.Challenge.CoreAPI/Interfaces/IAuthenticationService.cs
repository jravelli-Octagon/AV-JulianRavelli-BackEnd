using AmericanVirtual.Weather.Challenge.Common.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AmericanVirtual.Weather.Challenge.CoreAPI.Interfaces
{
    public interface IAuthenticationService
    {
        Task<ResponseResultDTO> Authenticate(UserDTO userDTO, string recaptchaToken);
        Task<ResponseResultDTO> RecoverPassword(string email, bool isBillingPassChange = false);
        Task<ResponseResultDTO> AssignPassword(string hash, string newPassword);
        Task<ResponseResultDTO> RefreshToken(string token, string refreshToken);
        Task<ResponseResultDTO> Create(UserDTO userDTO);
    }
}