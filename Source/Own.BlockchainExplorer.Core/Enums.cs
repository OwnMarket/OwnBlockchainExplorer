namespace Own.BlockchainExplorer.Core
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
            ValidatorReward,
            StakingReward,
            DepositTaken,
            DepositGiven,
            Action,
            StakeReturned
        }

        public enum TxStatus
        {
            Success,
            Failure,
            Pending
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
    }
}
