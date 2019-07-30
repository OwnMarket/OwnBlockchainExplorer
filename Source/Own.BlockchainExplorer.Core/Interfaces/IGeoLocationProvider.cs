using Own.BlockchainExplorer.Common.Framework;
using System.Threading.Tasks;
using Own.BlockchainExplorer.Core.Dtos.Api;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IGeoLocationProvider
    {
        Task<Result<GeoLocationDto>> GetGeoLocation(string ipAddress);
    }
}
