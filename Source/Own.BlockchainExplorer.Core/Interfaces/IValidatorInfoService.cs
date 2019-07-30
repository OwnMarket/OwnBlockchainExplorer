using Own.BlockchainExplorer.Common.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using Own.BlockchainExplorer.Core.Dtos.Api;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IValidatorInfoService
    {
        Result<ValidatorInfoDto> GetValidatorInfo(string blockchainAddress);
        Result<IEnumerable<StakeDto>> GetStakesInfo(string blockchainAddress, int page, int limit);
        Result<IEnumerable<ValidatorInfoShortDto>> GetValidators(int page, int limit);
        Task<Result<IEnumerable<ValidatorGeoInfoDto>>> GetValidatorsMap();
    }
}
