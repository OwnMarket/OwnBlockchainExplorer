using Newtonsoft.Json.Linq;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Scanning;
using Own.BlockchainExplorer.Core.Models;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IImportService
    {
        Result<Address> ImportAddress(string blockchainAddress, long nonce);
        Result<Block> ImportBlock(BlockDto blockDto);
        Result<Transaction> ImportTx(TxDto txDto, long timestamp);
        Result<Equivocation> ImportEquivocation(EquivocationDto equivocationDto, long blockId);

        Result<BlockchainEvent> ImportStakingRewardEvent(StakingRewardDto stakingRewardDto, long blockId);
        Result<BlockchainEvent> ImportValidatorRewardEvent(decimal reward, long blockId, string blockchainAddress);
        Result<BlockchainEvent> ImportDepositTakenEvent(EquivocationDto equivocationDto, long blockId, long equivocationId);
        Result<IEnumerable<BlockchainEvent>> ImportDepositGivenEvents(
            EquivocationDto equivocationDto,
            long blockId,
            long equivocationId);

        Result<TxAction> ImportAction(ActionDto actionDto, int actionNumber);
        Result<IEnumerable<BlockchainEvent>> ImportEvents(
            TxAction action,
            Address senderAddress,
            Block block,
            Transaction tx,
            JObject actionDataObj);
    }
}
