using System;
using System.Threading.Tasks;
using WayToCol.Common.Contracts.Responses;

namespace WayToCol.EstateFile.Service.Repository
{
    public interface ITokenRepository
    {
        Task<ServerResponse<string>> GetTokenAsync(dynamic data, TimeSpan timeSpan);
    }
}