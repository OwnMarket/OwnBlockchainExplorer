using Microsoft.AspNetCore.Mvc;
using Own.BlockchainExplorer.Api.Common;
using Own.BlockchainExplorer.Core.Interfaces;

namespace Own.BlockchainExplorer.Api.Controllers
{
    [Route("stat")]
    public class StatController : OwnController
    {
        private readonly IStatService _statService;

        public StatController(IStatService statService)
        {
            _statService = statService;
        }

        [HttpGet]
        [Route("tx-per-day")]
        public IActionResult GetTxPerDay(int numberOfDays = 7)
        {
            return ApiResult(_statService.GetTxPerDay(numberOfDays), r => NotFound(r));
        }

        [HttpGet]
        [Route("validator-stats")]
        public IActionResult GetValidatorStats([FromQuery] int numberOfDays = 7)
        {
            return ApiResult(_statService.GetValidatorStats(numberOfDays), r => NotFound(r));
        }

    }
}
