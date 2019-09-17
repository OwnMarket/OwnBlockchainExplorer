using Microsoft.AspNetCore.Mvc;
using Own.BlockchainExplorer.Api.Common;
using Own.BlockchainExplorer.Core.Interfaces;
using System.Threading.Tasks;

namespace Own.BlockchainExplorer.Api.Controllers
{
    [Route("stats")]
    public class StatController : OwnController
    {
        private readonly IStatService _statService;

        public StatController(IStatService statService)
        {
            _statService = statService;
        }

        [HttpGet]
        [Route("txs-per-day")]
        public IActionResult GetTxPerDay(int numberOfDays = 7)
        {
            return ApiResult(_statService.GetTxPerDay(numberOfDays), r => NotFound(r));
        }

        [HttpGet]
        [Route("validators")]
        public IActionResult GetValidatorStats([FromQuery] int numberOfDays = 7)
        {
            return ApiResult(_statService.GetValidatorStats(numberOfDays), r => NotFound(r));
        }

        [HttpGet]
        [Route("top-addresses")]
        public IActionResult GetTopAddresses(
            [FromQuery] int page = 1,
            [FromQuery] int limit = 50)
        {
            return ApiResult(_statService.GetTopAddresses(page, limit), r => NotFound(r));
        }

        [HttpGet]
        [Route("supply")]
        public async Task<IActionResult> GetChxSupplyAsync()
        {
            return await ApiResultAsync(_statService.GetChxSupply());
        }

        [HttpGet]
        [Route("supply/total")]
        public async Task<IActionResult> GetTotalChxSupplyAsync()
        {
            return await DataOrApiResultAsync(_statService.GetTotalChxSupply());
        }

        [HttpGet]
        [Route("supply/circulating")]
        public async Task<IActionResult> GetCirculatingChxSupplyAsync()
        {
            return await DataOrApiResultAsync(_statService.GetCirculatingChxSupply());
        }
    }
}
