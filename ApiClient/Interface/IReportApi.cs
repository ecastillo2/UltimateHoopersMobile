using Domain;
using Domain.DtoModel;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UltimateHoopers.Models;

namespace WebAPI.ApiClients
{
    /// <summary>
    /// Interface for Run API operations
    /// </summary>
    public interface IReportApi
    {
        /// <summary>
        /// Get all Runs
        /// </summary>
        Task<ReportDto> StreamAllCountsAsync(string accessToken, CancellationToken cancellationToken = default);

       
        
    }
}