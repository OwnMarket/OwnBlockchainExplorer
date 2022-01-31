using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Own.BlockchainExplorer.Api.Common;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Api.Controllers
{
    [Route("assets")]
    public class AssetsController : OwnController
    {
        private readonly IAssetService _assetService;
        private readonly IAssetBridgeService _assetBridgeService;

        public AssetsController(IAssetService assetService, IAssetBridgeService assetBridgeService)
        {
            _assetService = assetService;
            _assetBridgeService = assetBridgeService;
        }
        
        [HttpGet]
        public IActionResult GetAssets([FromQuery] string accountHash, [FromQuery] int page = 1, [FromQuery] int limit = 50) =>
            ApiResult(_assetService.GetAssets(accountHash, page, limit));
        
        [HttpGet]
        [Route("{assetHash}")]
        public IActionResult GetAssetInfo([FromRoute] string assetHash) =>
            ApiResult(_assetService.GetAssetInfo(assetHash));
        
        [HttpGet]
        [Route("{assetHash}/transfers")]
        public IActionResult GetAssetTransfers([FromRoute] string assetHash, [FromQuery] int page = 1, [FromQuery] int limit = 50) =>
            ApiResult(_assetService.GetAssetTransfers(assetHash, page, limit));
        
        [HttpGet]
        [Route("{assetHash}/transfers/bridge")]
        public Task<IActionResult> GetBridgeAssetTransfers([FromRoute] string assetHash) =>
            ApiResultAsync(_assetBridgeService.GetBridgeTransferStats(assetHash));
        
        [HttpGet]
        [Route("{assetHash}/holders")]
        public IActionResult GetAssetHolders([FromRoute] string assetHash, [FromQuery] int page = 1, [FromQuery] int limit = 50) =>
            ApiResult(_assetService.GetAssetHolders(assetHash, page, limit));
    }
}