// ***********************************************************************
// Assembly         : MteSDRTest.Server
// Author           : Eclypses Developer
// Created          : 07-20-2022
//
// Last Modified By : Eclypses Developer
// Last Modified On : 07-28-2022
// ***********************************************************************
// <copyright file="IDataExchangeService.cs" company="MteSDRTest.Server">
//     Copyright (c) Eclypses Inc.. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Threading.Tasks;
using MteSDRTest.Common.Models;

namespace MteSDRTest.Server.Services {
    /// <summary>
    /// Interface IDataExchangeService.
    /// </summary>
    public interface IDataExchangeService {
        /// <summary>
        /// Retrieves the value by updating the value property of the incoming argument.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>Task.</returns>
        Task RetrieveValue(DataExchangeModel data);
    }
}
