using Microsoft.AspNetCore.Mvc;
using Own.BlockchainExplorer.Api.Common;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Api.Controllers
{
    public class AddressInfoController : OwnController
    {
        private readonly IAddressInfoService _addressInfoService;

        public AddressInfoController(IAddressInfoService addressInfoService)
        {
            _addressInfoService = addressInfoService;
        }

        [HttpGet]
        [Route("address/{blockchainAddress}")]
        public IActionResult GetAddressInfo(string blockchainAddress)
        {
            return ApiResult(_addressInfoService.GetAddressInfo(blockchainAddress), r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/balance/total")]
        public IActionResult GetTotalChxBalanceInfo(string blockchainAddress)
        {
            return DataOrApiResult(_addressInfoService.GetTotalChxBalanceInfo(blockchainAddress));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/balance/available")]
        public IActionResult GetAvailableChxBalanceInfo(string blockchainAddress)
        {
            return DataOrApiResult(_addressInfoService.GetAvailableChxBalanceInfo(blockchainAddress));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/accounts")]
        public IActionResult GetAccountsInfo(
            string blockchainAddress,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50,
            [FromQuery] bool? isActive = null)
        {
            return ApiResult(
                _addressInfoService.GetAccountsInfo(blockchainAddress, page, limit, isActive),
                r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/assets")]
        public IActionResult GetAssetsInfo(
            string blockchainAddress,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50,
            [FromQuery] bool? isActive = null)
        {
            return ApiResult(
                _addressInfoService.GetAssetsInfo(blockchainAddress, page, limit, isActive),
                r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/delegated-stakes")]
        public IActionResult GetDelegatedStakesInfo(
            string blockchainAddress,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50)
        {
            return ApiResult(_addressInfoService.GetDelegatedStakesInfo(blockchainAddress, page, limit),
                r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/received-stakes")]
        public IActionResult GetReceivedStakesInfo(
            string blockchainAddress,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50)
        {
            return ApiResult(_addressInfoService.GetReceivedStakesInfo(blockchainAddress, page, limit),
                r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/events")]
        public IActionResult GetEventsInfo(
            string blockchainAddress,
            [FromQuery] string filter = "",
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50)
        {
            return ApiResult(_addressInfoService.GetEventsInfo(blockchainAddress, filter, page, limit), r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/events/download")]
        public IActionResult GetEventsCSV(string blockchainAddress)
        {
            var fileResult = _addressInfoService.GetEventsCSV(blockchainAddress);
            if (fileResult.Failed)
                return ApiResult(fileResult, r => NotFound(r));

            return File(fileResult.Data.Content, fileResult.Data.Type, fileResult.Data.Name);
        }
    }
}
