using System.Linq;
using System;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Interfaces;
using Own.BlockchainExplorer.Core.Models;
using Own.BlockchainExplorer.Domain.Common;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class StatService : DataService, IStatService
    {
        public StatService(
            IUnitOfWorkFactory unitOfWorkFactory,
            IRepositoryFactory repositoryFactory)
            : base(unitOfWorkFactory, repositoryFactory)
        {
        }

        public Result<IEnumerable<KeyValuePair<DateTime, int>>> GetTxPerDay(int numberOfDays)
        {
            using (var uow = NewUnitOfWork())
            {
                var currentDate = DateTime.UtcNow.Date;
                var minDate = currentDate.AddDays(-1 * numberOfDays);

                var result = NewRepository<Transaction>(uow)
                    .Get(t => GetDate(t.Timestamp) > minDate)
                    .GroupBy(t => GetDate(t.Timestamp))
                    .Select(g => new KeyValuePair<DateTime, int>(g.Key, g.Count()))
                    .ToList();

                var tempDate = minDate.AddDays(1);

                while (tempDate <= currentDate)
                {
                    if (!result.Exists(p => p.Key == tempDate))
                        result.Add(new KeyValuePair<DateTime, int>(tempDate, 0));
                    tempDate = tempDate.AddDays(1);
                }

                return Result.Success(result
                    .OrderByDescending(p => p.Key)
                    .AsEnumerable());
            }
        }

        private DateTime GetDate(long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0).AddMilliseconds(timestamp).Date;
        }

  
    }
}
