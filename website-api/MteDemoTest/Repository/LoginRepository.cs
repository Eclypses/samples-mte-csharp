using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MteDemoTest.Helpers;
using MteDemoTest.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MteDemoTest.Repository
{
    public class LoginRepository : ILoginRepository
    {
        #region Login Repository Constructor
        private readonly ILogger<LoginRepository> _logger;
        private readonly IMteStateHelper _stateHelper;
        private readonly IAuthHelper _authHelper;

        /// <summary>
        /// Login Repository
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="stateHelper"></param>
        /// <param name="authHelper"></param>
        public LoginRepository(ILogger<LoginRepository> logger, IMteStateHelper stateHelper, IAuthHelper authHelper)
        {
            _logger = logger;
            _stateHelper = stateHelper;
            _authHelper = authHelper;
        }
        #endregion

        #region UserLogin
        /// <summary>
        /// User Login
        /// Decodes incoming message and validates user
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="encodedInput"></param>
        /// <returns></returns>
        public ResponseModel<string> UserLogin(string clientId, string encodedInput)
        {
            ResponseModel<string> response = new ResponseModel<string>();
            try
            {
                //------------------------------
                // decode the incoming message
                //------------------------------
                ResponseModel<string> decodedResponse = _stateHelper.DecodeMessage(encodedInput, clientId);
                if (!decodedResponse.Success)
                {
                    response.ResultCode = decodedResponse.ResultCode;
                    response.Message = decodedResponse.Message;
                    response.Success = decodedResponse.Success;
                    return response;
                }
                LoginModel login = JsonSerializer.Deserialize<LoginModel>(decodedResponse.Data, Constants.JsonOptions);
                if (login == null)
                {
                    response.Message = "Login cannot be null.";
                    response.Success = false;
                    response.ResultCode = Constants.RC_VALIDATION_ERROR;
                    return response;
                }
                if (string.IsNullOrWhiteSpace(login.UserName))
                {
                    response.Message = "Username cannot be blank.";
                    response.Success = false;
                    response.ResultCode = Constants.RC_VALIDATION_ERROR;
                    return response;
                }
                if (string.IsNullOrWhiteSpace(login.Password))
                {
                    response.Message = "Password cannot be blank.";
                    response.Success = false;
                    response.ResultCode = Constants.RC_VALIDATION_ERROR;
                    return response;
                }
                EndUserCredentials result = _authHelper.Authenticate(login, clientId).Result;
                if (result == null)
                {
                    response.Success = false;
                    response.ResultCode = StatusCodes.Status401Unauthorized.ToString();
                    response.Message = "The user email or password is not correct";
                    response.Data = null;
                    return response;
                }
                else
                {
                    ResponseModel<string> encodeResponse = _stateHelper.EncodeMessage($"{login.UserName} successfully logged in.", clientId);
                    if (!encodeResponse.Success)
                    {
                        response.Message = encodeResponse.Message;
                        response.Success = encodeResponse.Success;
                        response.ResultCode = encodeResponse.ResultCode;
                        response.access_token = result.Token;
                        return response;
                    }

                    response.Data = encodeResponse.Data;
                    response.Message = Constants.STR_SUCCESS;
                    _logger.LogInformation($"{login.UserName} successfully logged in.");
                    response.access_token = result.Token;
                }
            }
            catch (Exception ex)
            {
                response.Message = $"Exception uploading file with MTE: {ex.Message}";
                response.Success = false;
                response.ResultCode = Constants.RC_REPOSITORY_EXCEPTION;
                _logger.LogError(ex.Message);
            }
            return response;
        } 
        #endregion
    }
}
