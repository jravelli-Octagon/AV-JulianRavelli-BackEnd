
using AmericanVirtual.Weather.Challenge.Common.DTOs;
using AmericanVirtual.Weather.Challenge.Common.Enums;
using AmericanVirtual.Weather.Challenge.Common.Extensions;
using AmericanVirtual.Weather.Challenge.Common.Helper;
using AmericanVirtual.Weather.Challenge.Common.Types;
using AmericanVirtual.Weather.Challenge.CoreAPI.Exceptions;
using AmericanVirtual.Weather.Challenge.CoreAPI.Helper;
using AmericanVirtual.Weather.Challenge.CoreAPI.Interfaces;
using AmericanVirtual.Weather.Challenge.CoreAPI.Mapper;
using AmericanVirtual.Weather.Challenge.Database.Model;
using AmericanVirtual.Weather.Challenge.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AmericanVirtual.Weather.Challenge.CoreAPI.Services
{
    //! Servicio de autenticación. 
    /*!
    */
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IUnitOfWork _UOW;
        private readonly IConfiguration _configuration;
        private readonly IRepository<User> _repo;
        protected readonly IAutoMapper _mapper;

        //! Servicio de autenticación
        /*!
        */
        public AuthenticationService(IUnitOfWork uow, IConfiguration configuration, IAutoMapper mapper)
        {
            _UOW = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = _UOW.GetRepository<User>();
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<ResponseResultDTO> Authenticate(UserDTO userDTO, string recaptchaToken)
        {
            _ = userDTO ?? throw new ArgumentNullException(nameof(userDTO));

            try
            {
                bool validateRecaptchat = await ValidateRecaptcha(recaptchaToken).ConfigureAwait(false);


                if (!validateRecaptchat)
                {
                    throw new ValidationException("INVALID_RECAPTCHA");
                }
                else
                {
                    userDTO.Username = userDTO.Username.ToUpper();

                    User user = await _repo.Single(u => u.Username.ToUpper().Equals(userDTO.Username)).ConfigureAwait(false);

                    if (user == null)
                    {
                        throw new ValidationException("INCORRECT_USER_OR_PASSWORD");
                    }

                    if (user.CurrentState != eUserStates.ACTIVE.ToString())
                    {
                        throw new ValidationException("USER_IS_BLOCKED");
                    }

                    await VerifyPassword(user, userDTO).ConfigureAwait(false);

                    string token = await GenerateToken(user).ConfigureAwait(false);


                    user.CreateAuthenticationToken(token);
                    user.Login();

                    _repo.Update(user);
                    await _UOW.SaveChangesAsync().ConfigureAwait(false);

                    return ResponseResultDTO.Ok(new UserDTO()
                    {
                        Token = token,
                        RefreshToken = user.AuthenticationTokens.LastOrDefault()!.RefreshToken,
                    });

                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async Task VerifyPassword(User user, UserDTO userDTO)
        {
            PasswordPolicy passwordPolicies = await _UOW.GetRepository<PasswordPolicy>().Single().ConfigureAwait(false);

            if (DateTime.Now.AddDays(passwordPolicies.NumberOfDaysUntilExpire * -1) > user.PasswordChangeDate)
            {
                try
                {
                    await BlockUser(user).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    throw new ValidationException("PASSWORD_EXPIRED");
                }
            }

            if (VerifyHashedPassword(user.Password, userDTO.Password) == PasswordVerificationResult.Failed)
            {
                user.FailedAttempts += 1;

                if (user.FailedAttempts > passwordPolicies.NumberOfFailedAttemptsAllowed)
                {
                    await BlockUser(user).ConfigureAwait(false);
                }

                _repo.Update(user);
                await _UOW.SaveChangesAsync().ConfigureAwait(false);

                throw new ValidationException("INVALID_USER_OR_PASSWORD");
            }
        }

        public async Task<ResponseResultDTO> RefreshToken(string accessToken, string refreshToken)
        {
            try
            {
                if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                {
                    throw new ValidationException("TOKEN_CANNOT_BE_EMPTY");
                }

                AuthenticationToken authenticationToken = await _UOW.GetRepository<AuthenticationToken>().Single(t => t.AccessToken == accessToken && t.RefreshToken == refreshToken,
                    include: include => include.Include(x => x.User)).ConfigureAwait(false);

                if (authenticationToken == null)
                {
                    throw new ValidationException("CANNOT_REFRESH_TOKEN");
                }
                if (authenticationToken.RefreshTokenExpiryTime < DateTime.Now)
                {
                    throw new ValidationException("REFRESH_TOKEN_EXPIRED");
                }

                string token = await GenerateToken(authenticationToken.User).ConfigureAwait(false);
                authenticationToken.User.CreateAuthenticationToken(token);
                // user.Login();

                _repo.Update(authenticationToken.User);
                await _UOW.SaveChangesAsync().ConfigureAwait(false);

                return ResponseResultDTO.Ok(new UserDTO()
                {
                    Token = token,
                    RefreshToken = authenticationToken.User.AuthenticationTokens.LastOrDefault()!.RefreshToken,
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<string> GenerateToken(User user)
        {
            byte[] key = Encoding.ASCII.GetBytes(_configuration.GetSection("TokenSecret").Value);
            IList<Claim> claims = BuildClaims(user.ID, user.Username);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(20), // addMinutes 5 para qa
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345")),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }

        private async Task BlockUser(User user)
        {
            user.ChangeUserState(eUserStates.BLOCKED.ToString());

            _repo.Update(user);
            await _UOW.SaveChangesAsync().ConfigureAwait(false);

            throw new ValidationException("BLOCKED_USER");
        }

        //! Función recuperar contraseña
        /*!
          \param email.
        */
        public async Task<ResponseResultDTO> RecoverPassword(string email, bool isBillingPassChange = false)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    throw new ValidationException("EMAIL_NOT_ENTERED");
                }

                User user = await _repo.Single(x => x.Email == email).ConfigureAwait(false);

                if (user == null)
                {
                    return ResponseResultDTO.Ok(new { timeStamp = DateTime.Now });
                }

                if (user.CurrentState.Equals(eUserStates.DELETED.ToString()))
                {
                    return ResponseResultDTO.Ok(new { timeStamp = DateTime.Now });
                }

                string ticketType = eTicketType.PASSWORD_RECOVERY.ToString();

                UserTicket ticket = await _UOW.GetRepository<UserTicket>().Single(x => x.IsActive && x.TicketType == ticketType && x.User.ID == user.ID,
                    include: include => include.Include(x => x.User)).ConfigureAwait(false);

                string hash;

                if (ticket == null)
                {
                    hash = await CreateTicket(user, ticketType).ConfigureAwait(false);
                }
                else
                {
                    hash = ticket.Hash;
                }

            // COMENTE EL ENVIO DE PASS DEL RECUPERO PORQUE NO TENGO UN SERVICIO DE EMAIL PARA INTEGRAR.
                // await SendRecoverPasswordEmail(user.Email, user.Username, hash).ConfigureAwait(false);

                return ResponseResultDTO.Ok(new { timeStamp = DateTime.Now });
            }
            catch (Exception)
            {
                throw;
            }
        }

    // COMENTE EL ENVIO DE PASS DEL RECUPERO PORQUE NO TENGO UN SERVICIO DE EMAIL PARA INTEGRAR.
        //public async Task<bool> SendRecoverPasswordEmail(string to, string userName, string hash)
        //{
        //    try
        //    {
        //        hash = EncodingHelper.Base64Encode(hash);
        //        string urlBase = ConfigurationHelper.GetBaseURL(_configuration);
        //        string path = Directory.GetCurrentDirectory() + "\\Templates\\recoverypassword.html";
        //        string body = File.ReadAllText(path, Encoding.UTF8)
        //            .Replace("#generatedUrl", $"{urlBase}/password-change?hash={hash}")
        //            .Replace("#usuario", userName);

        //        return await EmailHelper.SendEmail(_configuration, to, "RECUPERAR CONTRASEÑA", body, body).ConfigureAwait(false);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        private async Task<string> CreateTicket(User user, string ticketType)
        {
            string hash = user.CreateTicket(ticketType);

            _repo.Update(user);
            await _UOW.SaveChangesAsync().ConfigureAwait(false);

            return hash;
        }

        //! Función asignar contraseña
        /*!
          \param hash.
          \param newPassword.
        */
        public async Task<ResponseResultDTO> AssignPassword(string hash, string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(hash))
                {
                    throw new ValidationException("USER_NOT_EXISTS");
                }

                hash = EncodingHelper.Base64Decode(hash);

                //En este metodo nos va a llegar un hash, que se envio por email previamente.
                string decryptedHash = CryptoHelper.Decrypt256(hash);
                HashDTO info = JsonConvert.DeserializeObject<HashDTO>(decryptedHash);

                var user = await _repo.Single(x => x.Username == info.Username).ConfigureAwait(false);

                UserTicket ticket = await _UOW.GetRepository<UserTicket>().Single(x => x.IsActive == true && x.Hash == hash && x.User.ID == user.ID).ConfigureAwait(false);

                if (ticket == null)
                {
                    throw new ValidationException("USER_DOES_NOT_HAVE_ACTIVE_TICKET");
                }

                PasswordPolicy passwordPolicies = await _UOW.GetRepository<PasswordPolicy>().Single().ConfigureAwait(false);

                if (!PasswordValidateHelper.Validate(passwordPolicies, newPassword))
                {
                    throw new ValidationException("PASSWORD_NOT_VALID");
                }

                user.ChangePassword(newPassword);

                if (user.CurrentState == eUserStates.BLOCKED.ToString())
                {
                    user.ChangeUserState(eUserStates.ACTIVE.ToString());
                }

                ticket.Desactive();

                _UOW.GetRepository<UserTicket>().Update(ticket);
                _repo.Update(user);

                await _UOW.SaveChangesAsync().ConfigureAwait(false);

                return ResponseResultDTO.Ok(new { timeStamp = user.PasswordChangeDate });
            }
            catch (Exception)
            {
                throw;
            }
        }

        private IList<Claim> BuildClaims(long userID, string userName)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(UserClaimTypes.UserId, userID.ToString(CultureInfo.CurrentCulture), ClaimValueTypes.Integer64),
                new Claim(UserClaimTypes.UserName, userName, ClaimValueTypes.String),
            };

            IList<string> grantedPermissions = new List<string>();
            grantedPermissions.Add("WEATHER");

            claims.Add(new Claim(UserClaimTypes.PackedPermission,
                grantedPermissions.PackToString(":"), ClaimValueTypes.String));

            return claims;
        }

        //! Función verificar hash de contraseña
        /*!
          \param hashedPassword
          \param providedPassword
        */
        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (hashedPassword == null || providedPassword == null)
            {
                return PasswordVerificationResult.Failed;
            }

            byte[] decodedHashedPassword = Convert.FromBase64String(hashedPassword);

            if (decodedHashedPassword.Length == 0)
            {
                return PasswordVerificationResult.Failed;
            }

            // Verify hashing format.
            if (decodedHashedPassword[0] != 0x01)
            {
                // Unknown format header.
                return PasswordVerificationResult.Failed;
            }

            if (CryptoHelper.VerifyHashedPasswordInternal(decodedHashedPassword, providedPassword))
            {
                return PasswordVerificationResult.Success;
            }

            return PasswordVerificationResult.Failed;
        }

        private async Task<bool> ValidateRecaptcha(string recaptchaToken)
        {

            var siteKey = _configuration.GetSection("recaptchaPrivate").Value;

            //string enviroment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            //if (enviroment == "Development")
            //{
            //    return true;
            //}

            var client = new RestClient("https://www.google.com/recaptcha/api/siteverify");
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddParameter("secret", siteKey);
            request.AddParameter("response", recaptchaToken);
            RestResponse response = (RestResponse)client.Execute(request);
            var recaptchaResult = JsonConvert.DeserializeObject<CaptchaResponseDTO>(response.Content);


            if (!recaptchaResult.Success)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<ResponseResultDTO> Create(UserDTO userDTO)
        {
            try
            {
                User user = _mapper.Map<UserDTO, User>(userDTO);
                user.ChangePassword(user.Password);     // ESTO POR SEGURIDAD CLARAMENTE NO DEBERIA EXISTIR. ES PARA NO COMPLEJIZAR UN CHALLENGE.
                user.PasswordChangeDate = DateTime.UtcNow;
                _repo.Add(user);

                await _UOW.SaveChangesAsync().ConfigureAwait(false);

                return ResponseResultDTO.Ok(userDTO);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}