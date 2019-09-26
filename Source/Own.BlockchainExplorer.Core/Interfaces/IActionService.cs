using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.ActionData;
using Own.BlockchainExplorer.Core.Models;
using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IActionService
    {
        Result TransferChx(
              ref List<BlockchainEvent> events,
              TransferChxData actionData,
              IUnitOfWork uow,
              Address senderAddress);

        Result DelegateStake(
            ref List<BlockchainEvent> events,
            DelegateStakeData actionData,
            IUnitOfWork uow,
            Address senderAddress);

        Result ConfigureValidator(
            List<BlockchainEvent> events,
            ConfigureValidatorData actionData,
            IUnitOfWork uow,
            Address senderAddress);

        Result RemoveValidator(ref List<BlockchainEvent> events, IUnitOfWork uow, Address senderAddress);

        Result SetAssetCode(List<BlockchainEvent> events, SetAssetCodeData actionData, IUnitOfWork uow);

        Result SetAssetController(
            ref List<BlockchainEvent> events,
            SetAssetControllerData actionData,
            IUnitOfWork uow,
            Address senderAddress);

        Result SetAccountController(
            ref List<BlockchainEvent> events,
            SetAccountControllerData actionData,
            IUnitOfWork uow,
            Address senderAddress);

        Result TransferAsset(
            ref List<BlockchainEvent> events,
            TransferAssetData actionData,
            IUnitOfWork uow);

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
            ref List<BlockchainEvent> events,
            ChangeKycControllerAddressData actionData,
            IUnitOfWork uow,
            Address senderAddress);

        Result AddKycProvider(
            ref List<BlockchainEvent> events,
            AddKycProviderData actionData,
            IUnitOfWork uow,
            Address senderAddress);

        Result RemoveKycProvider(
            ref List<BlockchainEvent> events,
            RemoveKycProviderData actionData,
            IUnitOfWork uow,
            Address senderAddress);
    }
}
