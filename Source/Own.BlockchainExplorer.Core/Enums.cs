﻿namespace Own.BlockchainExplorer.Core
{
    namespace Enums
    {
        public enum UnitOfWorkMode
        {
            ReadOnly,
            Writable
        }

        public enum EventType
        {
            Action,
            ValidatorReward,
            StakingReward,
            DepositTaken,
            DepositGiven,
            StakeReturned,
            DormantValidatorDeactivated
        }

        public enum TxStatus
        {
            Pending,
            Success,
            Failure
        }

        public enum TransferType
        {
            FromNative,
            ToNative
        }

        public enum BlockchainCode
        {
            Eth,
            Bsc
        }

        public enum ActionType
        {
            TransferChx,
            DelegateStake,
            ConfigureValidator,
            RemoveValidator,
            TransferAsset,
            CreateAssetEmission,
            CreateAsset,
            SetAssetCode,
            SetAssetController,
            CreateAccount,
            SetAccountController,
            SubmitVote,
            SubmitVoteWeight,
            SetAccountEligibility,
            SetAssetEligibility,
            ChangeKycControllerAddress,
            AddKycProvider,
            RemoveKycProvider
        }

        public enum SearchType
        {
            Address,
            Account,
            Asset,
            Transaction,
            Equivocation,
            Block
        }
    }
}
