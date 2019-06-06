﻿using Microsoft.AspNetCore.Mvc;
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
        public IActionResult GetAccountsInfo(string blockchainAddress, 
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 50)
        {
            return ApiResult(_addressInfoService.GetAccountsInfo(blockchainAddress, page, limit), r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/assets")]
        public IActionResult GetAssetsInfo(string blockchainAddress, 
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 50)
        {
            return ApiResult(_addressInfoService.GetAssetsInfo(blockchainAddress, page, limit), r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/delegated-stakes")]
        public IActionResult GetDelegatedStakesInfo(string blockchainAddress, 
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 50)
        {
            return ApiResult(_addressInfoService.GetDelegatedStakesInfo(blockchainAddress, page, limit), 
                r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/received-stakes")]
        public IActionResult GetReceivedStakesInfo(string blockchainAddress, 
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 50)
        {
            return ApiResult(_addressInfoService.GetReceivedStakesInfo(blockchainAddress, page, limit), 
                r => NotFound(r));
        }

        [HttpGet]
        [Route("address/{blockchainAddress}/events")]
        public IActionResult GetEventsInfo(string blockchainAddress,
            [FromQuery] int page = 1, 
            [FromQuery] int limit = 50)
        {
            return ApiResult(_addressInfoService.GetEventsInfo(blockchainAddress, page, limit), r => NotFound(r));
        }
    }
}
