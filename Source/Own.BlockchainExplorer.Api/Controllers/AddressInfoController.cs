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
        [Route("address/{blockchainAddress}/accounts")]
        public IActionResult GetAccountsInfo(string blockchainAddress)
        {
            return ApiResult(_addressInfoService.GetAccountsInfo(blockchainAddress), r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/assets")]
        public IActionResult GetAssetsInfo(string blockchainAddress)
        {
            return ApiResult(_addressInfoService.GetAssetsInfo(blockchainAddress), r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/delegated-stakes")]
        public IActionResult GetDelegatedStakesInfo(string blockchainAddress)
        {
            return ApiResult(_addressInfoService.GetDelegatedStakesInfo(blockchainAddress), r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/received-stakes")]
        public IActionResult GetReceivedStakesInfo(string blockchainAddress)
        {
            return ApiResult(_addressInfoService.GetReceivedStakesInfo(blockchainAddress), r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/events")]
        public IActionResult GetEventsInfo(string blockchainAddress)
        {
            return ApiResult(_addressInfoService.GetEventsInfo(blockchainAddress), r => NotFound(r));
        }
    }
}
