using Microsoft.AspNetCore.Mvc;
using Own.BlockchainExplorer.Api.Common;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Api.Controllers
{
    public class BlockInfoController : OwnController
    {
        private readonly IBlockInfoService _blockInfoService;

        public BlockInfoController(IBlockInfoService blockInfoService)
        {
            _blockInfoService = blockInfoService;
        }

        [HttpGet]
        [Route("block/{blockNumber}")]
        public IActionResult GetBlockInfo(long blockNumber)
        {
            return ApiResult(_blockInfoService.GetBlockInfo(blockNumber), r => NotFound(r));
        }

        [HttpGet]
        [Route("block/{blockNumber}/transactions")]
        public IActionResult GetTransactionsInfo(long blockNumber,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50)
        {
            return ApiResult(_blockInfoService.GetTransactionsInfo(blockNumber, page, limit), r => NotFound(r));
        }

        [HttpGet]
        [Route("block/{blockNumber}/equivocations")]
        public IActionResult GetEquivocationsInfo(long blockNumber,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50)
        {
            return ApiResult(_blockInfoService.GetEquivocationsInfo(blockNumber, page, limit), r => NotFound(r));
        }

        [HttpGet]
        [Route("block/{blockNumber}/staking-rewards")]
        public IActionResult GetStakingRewardInfo(long blockNumber,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50)
        {
            return ApiResult(_blockInfoService.GetStakingRewardInfo(blockNumber, page, limit), r => NotFound(r));
        }
    }
}
