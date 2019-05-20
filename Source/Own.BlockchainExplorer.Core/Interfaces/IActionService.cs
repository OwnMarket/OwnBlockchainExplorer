using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.ActionData;
using Own.BlockchainExplorer.Core.Models;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IActionService
    {
        Result TransferChx(
              List<BlockchainEvent> events,
              TransferChxData actionData,
              IUnitOfWork uow,
              Address senderAddress);

        Result DelegateStake(
            List<BlockchainEvent> events,
            DelegateStakeData actionData,
            IUnitOfWork uow,
            Address senderAddress);

        Result ConfigureValidator(
            List<BlockchainEvent> events,
            ConfigureValidatorData actionData,
            IUnitOfWork uow,
            Address senderAddress);

        Result RemoveValidator(List<BlockchainEvent> events, IUnitOfWork uow, Address senderAddress);

        Result SetAssetCode(List<BlockchainEvent> events, SetAssetCodeData actionData, IUnitOfWork uow);

        Result SetAssetController(
            List<BlockchainEvent> events,
            SetAssetControllerData actionData,
            IUnitOfWork uow);

        Result SetAccountController(
            List<BlockchainEvent> events,
            SetAccountControllerData actionData,
            IUnitOfWork uow);

        Result TransferAsset(List<BlockchainEvent> events, TransferAssetData actionData, IUnitOfWork uow);

        Result CreateAssetEmission(
            List<BlockchainEvent> events,
            CreateAssetEmissionData actionData,
            IUnitOfWork uow);

        Result CreateAsset(List<BlockchainEvent> events, IUnitOfWork uow, Address senderAddress, TxAction action);

        Result CreateAccount(List<BlockchainEvent> events, IUnitOfWork uow, Address senderAddress, TxAction action);

        Result SubmitVote(
            List<BlockchainEvent> events,
            SubmitVoteData actionData,
            IUnitOfWork uow);

        Result SubmitVoteWeight(
            List<BlockchainEvent> events,
            SubmitVoteWeightData actionData,
            IUnitOfWork uow);

        Result SetAccountEligibility(
            List<BlockchainEvent> events,
            SetAccountEligibilityData actionData,
            IUnitOfWork uow);

        Result SetAssetEligibility(
            List<BlockchainEvent> events,
            SetAssetEligibilityData actionData,
            IUnitOfWork uow);

        Result ChangeKycControllerAddress(
            List<BlockchainEvent> events,
            ChangeKycControllerAddressData actionData,
            IUnitOfWork uow);

        Result AddKycProvider(
            List<BlockchainEvent> events,
            AddKycProviderData actionData,
            IUnitOfWork uow);

        Result RemoveKycProvider(
            List<BlockchainEvent> events,
            RemoveKycProviderData actionData,
            IUnitOfWork uow);
    }
}
