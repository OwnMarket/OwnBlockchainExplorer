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
              Address senderAddress,
              IUnitOfWork uow);

        Result DelegateStake(
            ref List<BlockchainEvent> events,
            DelegateStakeData actionData,
            Address senderAddress,
            IUnitOfWork uow);

        Result ConfigureValidator(
            List<BlockchainEvent> events,
            ConfigureValidatorData actionData,
            Address senderAddress,
            IUnitOfWork uow);

        Result RemoveValidator(ref List<BlockchainEvent> events, Address senderAddress, IUnitOfWork uow);

        Result SetAssetCode(List<BlockchainEvent> events, SetAssetCodeData actionData, IUnitOfWork uow);

        Result SetAssetController(
            ref List<BlockchainEvent> events,
            SetAssetControllerData actionData,
            Address senderAddress,
            IUnitOfWork uow);

        Result SetAccountController(
            ref List<BlockchainEvent> events,
            SetAccountControllerData actionData,
            Address senderAddress,
            IUnitOfWork uow);

        Result TransferAsset(
            ref List<BlockchainEvent> events,
            TransferAssetData actionData,
            IUnitOfWork uow);

        Result CreateAssetEmission(
            List<BlockchainEvent> events,
            CreateAssetEmissionData actionData,
            IUnitOfWork uow);

        Result CreateAsset(
            List<BlockchainEvent> events,
            Address senderAddress,
            TxAction action,
            IUnitOfWork uow);

        Result CreateAccount(
            List<BlockchainEvent> events,
            Address senderAddress,
            TxAction action,
            IUnitOfWork uow);

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
            Address senderAddress,
            IUnitOfWork uow);

        Result AddKycProvider(
            List<BlockchainEvent> events,
            AddKycProviderData actionData,
            Address senderAddress,
            IUnitOfWork uow);

        Result RemoveKycProvider(
            List<BlockchainEvent> events,
            RemoveKycProviderData actionData,
            Address senderAddress,
            IUnitOfWork uow);
    }
}
