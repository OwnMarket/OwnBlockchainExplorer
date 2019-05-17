using Microsoft.AspNetCore.Mvc;
using Own.BlockchainExplorer.Api.Common;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Api.Controllers
{
    public class BlockchainInfoController : OwnController
    {
        private readonly IBlockchainInfoService _blockchainInfoService;

        public BlockchainInfoController(IBlockchainInfoService blockchainInfoService)
        {
            _blockchainInfoService = blockchainInfoService;
        }

        [HttpGet]
        [Route("address/{blockchainAddress}")]
        public IActionResult GetAddressInfo(string blockchainAddress)
        {
            return ApiResult(_blockchainInfoService.GetAddressInfo(blockchainAddress), r => NotFound(r));
        }

        [HttpGet]
        [Route("block/{blockNumber}")]
        public IActionResult GetBlockInfo(long blockNumber)
        {
            return ApiResult(_blockchainInfoService.GetBlockInfo(blockNumber), r => NotFound(r));
        }

        [HttpGet]
        [Route("tx/{txHash}")]
        public IActionResult GetTxInfo(string txHash)
        {
            return ApiResult(_blockchainInfoService.GetTxInfo(txHash), r => NotFound(r));
        }

        [HttpGet]
        [Route("equivocation/{equivocationProofHash}")]
        public IActionResult GetEquivocationInfo(string equivocationProofHash)
        {
            return ApiResult(_blockchainInfoService.GetEquivocationInfo(equivocationProofHash), r => NotFound(r));
        }

        [HttpGet]
        [Route("account/{accountHash}")]
        public IActionResult GetAccountInfo(string accountHash)
        {
            return ApiResult(_blockchainInfoService.GetAccountInfo(accountHash), r => NotFound(r));
        }

        [HttpGet]
        [Route("asset/{assetHash}")]
        public IActionResult GetAssetInfo(string assetHash)
        {
            return ApiResult(_blockchainInfoService.GetAssetInfo(assetHash), r => NotFound(r));
        }

        [HttpGet]
        [Route("validator/{blockchainAddress}")]
        public IActionResult GetValidatorInfo(string blockchainAddress)
        {
            return ApiResult(_blockchainInfoService.GetValidatorInfo(blockchainAddress), r => NotFound(r));
        }

        [HttpGet]
        [Route("blocks")]
        public IActionResult GetBlocks([FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            return ApiResult(_blockchainInfoService.GetBlocks(limit, page), r => NotFound(r));
        }

        [HttpGet]
        [Route("txs")]
        public IActionResult GetTransactions([FromQuery] int page = 1, [FromQuery] int limit = 50)
        {
            return ApiResult(_blockchainInfoService.GetTxs(limit, page), r => NotFound(r));
        }

        [HttpGet]
        [Route("validators")]
        public IActionResult GetValidators()
        {
            return ApiResult(_blockchainInfoService.GetValidators(), r => NotFound(r));
        }

        [HttpGet]
        [Route("search/{hash}")]
        public IActionResult Search(string hash)
        {
            return ApiResult(_blockchainInfoService.Search(hash), r => NotFound(r));
        }
    }
}
