using System.Collections.Generic;
using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.ActionData;
using Own.BlockchainExplorer.Core.Models;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IActionService
    {
        Result<List<BlockchainEvent>> TransferChx(
              BlockchainEvent senderEvent,
              TransferChxData actionData,
              Address senderAddress,
              IUnitOfWork uow);

        Result<List<BlockchainEvent>> DelegateStake(
            BlockchainEvent senderEvent,
            DelegateStakeData actionData,
            Address senderAddress,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> ConfigureValidator(
            ConfigureValidatorData actionData,
            Address senderAddress,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> RemoveValidator(BlockchainEvent senderEvent, Address senderAddress, IUnitOfWork uow);

        Result<List<BlockchainEvent>> SetAssetCode(BlockchainEvent senderEvent, SetAssetCodeData actionData, IUnitOfWork uow);

        Result<List<BlockchainEvent>> SetAssetController(
            BlockchainEvent senderEvent,
            SetAssetControllerData actionData,
            Address senderAddress,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> SetAccountController(
            BlockchainEvent senderEvent,
            SetAccountControllerData actionData,
            Address senderAddress,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> TransferAsset(
            BlockchainEvent senderEvent,
            TransferAssetData actionData,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> CreateAssetEmission(
            BlockchainEvent senderEvent,
            CreateAssetEmissionData actionData,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> CreateAsset(
            BlockchainEvent senderEvent,
            Address senderAddress,
            TxAction action,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> CreateAccount(
            BlockchainEvent senderEvent,
            Address senderAddress,
            TxAction action,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> SubmitVote(
            BlockchainEvent senderEvent,
            SubmitVoteData actionData,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> SubmitVoteWeight(
            BlockchainEvent senderEvent,
            SubmitVoteWeightData actionData,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> SetAccountEligibility(
            BlockchainEvent senderEvent,
            SetAccountEligibilityData actionData,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> SetAssetEligibility(
            BlockchainEvent senderEvent,
            SetAssetEligibilityData actionData,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> ChangeKycControllerAddress(
            BlockchainEvent senderEvent,
            ChangeKycControllerAddressData actionData,
            Address senderAddress,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> AddKycProvider(
            BlockchainEvent senderEvent,
            AddKycProviderData actionData,
            Address senderAddress,
            IUnitOfWork uow);

        Result<List<BlockchainEvent>> RemoveKycProvider(
            BlockchainEvent senderEvent,
            RemoveKycProviderData actionData,
            Address senderAddress,
            IUnitOfWork uow);
    }
}
