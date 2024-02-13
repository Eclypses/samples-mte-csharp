using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MteDemoTest.Models;
using MteDemoTest.Repository;
using System;
using System.Threading.Tasks;

namespace MteDemoTest.Controllers
{
    [ApiController]
    public class FileUploadController : ControllerBase
    {
        /// <summary>
        /// Add in dependencies
        /// </summary>
        private readonly IFileUploadRepository _fileUploadRepository;
        private readonly Helpers.IAuthHelper _authHelper;

        public FileUploadController(IFileUploadRepository fileUploadRepository, Helpers.IAuthHelper authHelper)
        {
            _fileUploadRepository = fileUploadRepository;
            _authHelper = authHelper;
        }

        #region FileUpload/nomte
        /// <summary>
        /// File Upload using NO MTE or login
        /// </summary>
        /// <param name="name">Name of file being uploaded</param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("FileUpload/nomte")]
        public async Task<ActionResult<ResponseModel<byte>>> FileUpload(string name)
        {
            EndUserCredentials endUser = null;
            try
            {
                //-------------------------------------
                // Get the user object out of the JWT
                //-------------------------------------
                endUser = await _authHelper.RetrieveEndUserCredentials(User);

                //--------------
                // Upload file
                //--------------
                ResponseModel<byte[]> result = await _fileUploadRepository.FileUpload(name, Request, false);
                //---------
                // Set JWT
                //---------
                if (endUser != null && !string.IsNullOrWhiteSpace(endUser.Token))
                {
                    result.access_token = endUser.Token;
                }
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                ResponseModel<byte[]> exceptionResponse = new ResponseModel<byte[]>
                {
                    Message = ex.Message,
                    ResultCode = Constants.RC_CONTROLLER_EXCEPTION,
                    Success = false
                };
                return new JsonResult(exceptionResponse);
            }
        }
        #endregion

        #region FileUploadMte
        /// <summary>
        /// File Upload with MTE but NO login
        /// </summary>
        /// <param name="name">Name of file being uploaded</param>
        /// <returns></returns>
        [HttpPost]
        [Route("FileUpload/mte")]
        [AllowAnonymous]
        public async Task<ActionResult<byte[]>> FileUploadMte(string name)
        {
            EndUserCredentials endUser = null;
            try
            {
                //-------------------------------------
                // Get the user object out of the JWT
                //-------------------------------------
                endUser = await _authHelper.RetrieveEndUserCredentials(User);

                //--------------
                // Upload file
                //--------------
                ResponseModel<byte[]> result = await _fileUploadRepository.FileUpload(name, Request, true);
                //---------
                // Set JWT
                //---------
                if (endUser != null && !string.IsNullOrWhiteSpace(endUser.Token))
                {
                    result.access_token = endUser.Token;
                }
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                ResponseModel<byte[]> exceptionResponse = new ResponseModel<byte[]>
                {
                    Message = ex.Message,
                    ResultCode = Constants.RC_CONTROLLER_EXCEPTION,
                    Success = false
                };
                return new JsonResult(exceptionResponse);
            }
        }
        #endregion

        #region FileUploadLoginMte
        /// <summary>
        /// File upload using MTE and requiring login
        /// </summary>
        /// <param name="name">Name of file uploading</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("FileUploadLogin/mte")]
        [Authorize(Roles = "Customer")]
        public async Task<ActionResult<byte[]>> FileUploadLoginMte(string name)
        {
            EndUserCredentials endUser = null;
            try
            {
                //-------------------------------------
                // Get the user object out of the JWT
                //-------------------------------------
                endUser = await _authHelper.RetrieveEndUserCredentials(User);

                //--------------
                // Upload file
                //--------------
                ResponseModel<byte[]> result = await _fileUploadRepository.FileUpload(name, Request, true);
                //---------
                // Set JWT
                //---------
                result.access_token = endUser.Token;
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                ResponseModel<byte[]> exceptionResponse = new ResponseModel<byte[]>
                {
                    Message = ex.Message,
                    ResultCode = Constants.RC_CONTROLLER_EXCEPTION,
                    Success = false
                };
                return new JsonResult(exceptionResponse);
            }
        } 
        #endregion
    }
}
