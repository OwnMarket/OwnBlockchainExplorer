using Newtonsoft.Json.Linq;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Scanning;
using Own.BlockchainExplorer.Core.Models;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IImportService
    {
        Result<Address> ImportAddress(string blockchainAddress, long nonce, IUnitOfWork uow);
        Result<Block> ImportBlock(BlockDto blockDto, IUnitOfWork uow);
        Result<Transaction> ImportTx(TxDto txDto, long timestamp, IUnitOfWork uow);
        Result<Equivocation> ImportEquivocation(EquivocationDto equivocationDto, long blockId, IUnitOfWork uow);

        Result<BlockchainEvent> ImportStakingRewardEvent(
            StakingRewardDto stakingRewardDto,
            long blockId,
            IUnitOfWork uow);
        Result<BlockchainEvent> ImportValidatorRewardEvent(
            decimal reward,
            long blockId,
            string blockchainAddress,
            IUnitOfWork uow);
        Result<BlockchainEvent> ImportDepositTakenEvent(
            EquivocationDto equivocationDto,
            long blockId,
            long equivocationId,
            IUnitOfWork uow);
        Result<IEnumerable<BlockchainEvent>> ImportDepositGivenEvents(
            EquivocationDto equivocationDto,
            long blockId,
            long equivocationId,
            IUnitOfWork uow);

        Result<TxAction> ImportAction(ActionDto actionDto, int actionNumber, IUnitOfWork uow);
        Result<IEnumerable<BlockchainEvent>> ImportEvents(
            TxAction action,
            Address senderAddress,
            long blockId,
            Transaction tx,
            JObject actionDataObj,
            IUnitOfWork uow);
    }
}
