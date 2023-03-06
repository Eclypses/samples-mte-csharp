using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MteDemoTest.Models;

namespace MteDemoTest.Repository
{
    public interface IFileUploadRepository
    {
        /// <summary>
        /// Handles the File Upload
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="request"></param>
        /// <param name="useMte"></param>
        /// <returns></returns>
        Task<ResponseModel<byte[]>> FileUpload(string fileName, HttpRequest request, bool useMte);
    }
}
