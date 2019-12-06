using Microsoft.AspNetCore.Mvc;
using Own.BlockchainExplorer.Api.Common;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Api.Controllers
{
    public class TxInfoController : OwnController
    {
        private readonly ITxInfoService _txInfoService;

        public TxInfoController(ITxInfoService txInfoService)
        {
            _txInfoService = txInfoService;
        }

        [HttpGet]
        [Route("tx/{txHash}")]
        public IActionResult GetTxInfo(string txHash)
        {
            return ApiResult(_txInfoService.GetTxInfo(txHash), r => NotFound(r));
        }

        [HttpGet]
        [Route("tx/{txHash}/actions")]
        public IActionResult GetActionsInfo(string txHash)
        {
            return ApiResult(_txInfoService.GetActionsInfo(txHash), r => NotFound(r));
        }
    }
}
