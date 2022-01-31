using Microsoft.AspNetCore.Mvc;
using Own.BlockchainExplorer.Api.Common;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Api.Controllers
{
    [Route("accounts")]
    public class AccountController : OwnController
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        
        [HttpGet]
        public IActionResult GetAccounts([FromQuery] int page = 1, [FromQuery] int limit = 50) =>
            ApiResult(_accountService.GetAccounts(page, limit));
        
        [HttpGet]
        [Route("{accountHash}")]
        public IActionResult GetAccountInfo([FromRoute] string accountHash) =>
            ApiResult(_accountService.GetAccountInfo(accountHash));
        
        [HttpGet]
        [Route("{accountHash}/transfers")]
        public IActionResult GetAccountTransfers([FromRoute] string accountHash, [FromQuery] int page = 1, [FromQuery] int limit = 50) =>
            ApiResult(_accountService.GetAccountTransfers(accountHash, page, limit));
        
        [HttpGet]
        [Route("{accountHash}/holdings")]
        public IActionResult GetAccountHoldings([FromRoute] string accountHash, [FromQuery] int page = 1, [FromQuery] int limit = 50) =>
            ApiResult(_accountService.GetAccountHoldings(accountHash, page, limit));
    }
}