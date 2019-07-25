using Microsoft.AspNetCore.Mvc;
using Own.BlockchainExplorer.Api.Common;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Api.Controllers
{
    public class ValidatorInfoController : OwnController
    {
        private readonly IValidatorInfoService _validatorInfoService;

        public ValidatorInfoController(IValidatorInfoService validatorInfoService)
        {
            _validatorInfoService = validatorInfoService;
        }

        [HttpGet]
        [Route("validators")]
        public IActionResult GetValidators([FromQuery] int page = 1, [FromQuery] int limit = 100)
        {
            return ApiResult(_validatorInfoService.GetValidators(page, limit), r => NotFound(r));
        }

        [HttpGet]
        [Route("validator/{blockchainAddress}")]
        public IActionResult GetValidatorInfo(string blockchainAddress)
        {
            return ApiResult(_validatorInfoService.GetValidatorInfo(blockchainAddress), r => NotFound(r));
        }

        [HttpGet]
        [Route("validator/{blockchainAddress}/stakes")]
        public IActionResult GetValidatorStakesInfo(
            string blockchainAddress,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50)
        {
            return ApiResult(_validatorInfoService.GetStakesInfo(blockchainAddress, page, limit), r => NotFound(r));
        }
    }
}
