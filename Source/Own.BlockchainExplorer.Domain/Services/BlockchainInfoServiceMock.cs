using Own.BlockchainExplorer.Common.Framework;
using Own.BlockchainExplorer.Core.Dtos.Api;
using Own.BlockchainExplorer.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Own.BlockchainExplorer.Domain.Services
{
    public class BlockchainInfoServiceMock : IBlockchainInfoService
    {
        public Result<AddressInfoDto> GetAddressInfo(string blockchainAddress)
        {
            if (blockchainAddress != "CHVegEXVwUhK2gbrqnMsYyNSVC7CLTM7qmQ")
                return Result.Failure<AddressInfoDto>("Address does not exist.");

            return Result.Success(new AddressInfoDto() {
                BlockchainAddress = "CHVegEXVwUhK2gbrqnMsYyNSVC7CLTM7qmQ",
                Nonce = 7,
                ChxBalanceInfo = new ChxBalanceInfoDto()
                {
                    AvailableBalance = 1399.993M,
                    DelegatedStakes = 100,
                    ValidatorDeposit = 0
                }
            });
        }

        public Result<BlockInfoDto> GetBlockInfo(long blockNumber)
        {
            return Result.Success(new BlockInfoDto() {
                BlockNumber = 121,
                Hash = "6pYM6BBGyQttPCTVmpqNpH4i4jE5KjgmaXCWuDJcbiLp",
                PreviousBlockHash = "EsEYC4f4xvNiSQLRX8P3GNzGeWm1smVvUD5U5oTnDw3b",
                ConfigurationBlockNumber = 100,
                Timestamp = DateTime.UtcNow,
                ValidatorAddress = "CHN5FmdEhjKHynhdbzXxsNB35oxL559gRLH",
                TxSetRoot = "9U4jKtneZ5CJ2qA3qzmiCbekz6j5ZaXSjGAPqaADscG1",
                TxResultSetRoot = "6Hb32QtPVJdRiubdGRb73WiExmk75rKP76jDfrBg6R9B",
                EquivocationProofsRoot = "9Vc4dQKFpQ8XP36q5TFAnpChvFmTg8UH6rpu3FqVn268",
                EquivocationProofResultsRoot  = "Gtcbiey3WwiRHrYuGc5ytcttEpMq19uY4oA2FAaMXwLc",
                StateRoot = "C6MYZeTZUKZwikUpTEADCYANRZXvMJquCTMSE2ztz78C",
                StakingRewardsRoot = "Ey9qZK4J4G2PK68ZFzyteP8dcUWCjcBiMZ46D7nH11pY",
                ConfigurationRoot = "8J9eC9A3jzmwCxg8VhjT84xAts8tVVm6RwzLKBhMd3D8",
                ConsensusRound = 0,
                Signatures = "GBfbNB8xQUaLoZDFDGtpAH3EYks2uFdaBErPJKAzR37DBkKDL4HEFNRYLPapbQuZ5JgDzUSXgU7iGJqy4sSJx4775;64y6JNqpeDqS927rgbCKbdqysAkeEfLC4KS5pAeDpuPL2SR1gsqFFMr5YmrcrPhk75dXeihWovUqctYPrDHjCejiP;664ZaikasKTrtp9BMnoJgEtFuVAgdEq56D9FJKvF1jsq7pFAEXmJu7Nt4FmWjfT8ncXQTCxhakUGgFiDHPjnE4xLG"
            });
        }

        public Result<TxInfoDto> GetTxInfo(string txHash)
        {
            return Result.Success(new TxInfoDto() {
                Hash = "6XTowWarMR1UjzAVfiMYs7hsKK9hPBagR7JtFn7nxgfK",
                SenderAddress = "CHVegEXVwUhK2gbrqnMsYyNSVC7CLTM7qmQ",
                BlockNumber = 121,
                Timestamp = DateTime.UtcNow,
                Nonce = 12,
                ExpirationTime = DateTime.UtcNow.AddSeconds(10),
                ActionFee = 0.01M,
                Status = "Success"
            });
        }

        public Result<EquivocationInfoDto> GetEquivocationInfo(string equivocationProofHash)
        {
            return Result.Success(new EquivocationInfoDto() {
                EquivocationProofHash = "BTXVBwuTXWTpPtJC71FPGaeC17NVhu9mS6JavqZqHbYH",
                BlockNumber = 123,
                ConsensusRound = 1,
                ConsensusStep = "vote",
                EquivocationValue1 = "9VMtBESNLXWFRQXrd2HbXc2CGWUkdyPQjAKP5MciU59k",
                EquivocationValue2 = "D8ViZH31RHBYrDfUhUC1DK49pY1dxCvgRMsbnS9Lbn3p",
                Signature1 = "E5nmjsHcL1hFmJEjphUhg6DBn6gyxYzrTKKtXvDGB8FhefQZQ6o5QJ1MRgXqqY97YMsCe8cs3muDF524Mq1Q9qTzG",
                Signature2 = "M4jAhLWup8fe6NVnUg193uqLzGdgFuo6XFP2pDZFWGNvK6LuwYRqwM8HBADatgTZreXz2oZr5GhA3kZqi2GhaHrZE",
                TakenDeposit = new DepositDto
                {
                    Amount = 5000,
                    BlockchainAddress = "CHT72YWjChhv5xYeDono6Nn4Z5Qe5Q7aRyq",
                },
                GivenDeposits = new List<DepositDto>()
                {
                    new DepositDto()
                    {
                        Amount = 1000,
                        BlockchainAddress = "CHc1zbyXodtHMEsixH7ZQEajY2Fun3ab5jy",
                    },
                    new DepositDto()
                    {
                        Amount = 1000,
                        BlockchainAddress = "CHVkbiNDYsZJTcUUtRDEucceRrZ8kbXgFCJ",
                    },
                    new DepositDto()
                    {
                        Amount = 1000,
                        BlockchainAddress = "CHZHzBNVMQweCiqYVueEYpmeJMax8382HFr",
                    },
                    new DepositDto()
                    {
                        Amount = 1000,
                        BlockchainAddress = "CHLwBmcHjN23HCNmLMag3sosxeW13h3cko6",
                    },
                    new DepositDto()
                    {
                        Amount = 1000,
                        BlockchainAddress = "CHNugxKAxMaPbKpx5yraBNoLh63icwqVa5Y",
                    }
                },
                IncludedInBlockNumber = 125
            });
        }

        public Result<AccountInfoDto> GetAccountInfo(string accountHash)
        {
            return Result.Success(new AccountInfoDto() {
                Hash = "wcpUPec7pNUKys9pkvPfhjkezekZ99GHpXavbS6M1R4",
                ControllerAddress = "CHLsVaYSPJGFi8BNGd6tP1VvB8UdKbVRDKD",
                ControllerAddresses = new List<ControllerAddressDto>()
                {
                    new ControllerAddressDto()
                    {
                        BlockchainAddress = "CHLsVaYSPJGFi8BNGd6tP1VvB8UdKbVRDKD"
                    },
                    new ControllerAddressDto()
                    {
                        BlockchainAddress = "CHMf4inrS8hnPNEgJVZPRHFhsDPCHSHZfAJ"
                    }
                },
                Holdings = new List<HoldingDto>()
                {
                    new HoldingDto()
                    {
                        AccountHash = "wcpUPec7pNUKys9pkvPfhjkezekZ99GHpXavbS6M1R4",
                        AssetHash = "BTXVBwuTXWTpPtJC71FPGaeC17NVhu9mS6JavqZqHbYH",
                        Balance = 100
                    },
                    new HoldingDto()
                    {
                        AccountHash = "wcpUPec7pNUKys9pkvPfhjkezekZ99GHpXavbS6M1R4",
                        AssetHash = "ETktHKf3kySqS6uTN321y1N5iBf1SYsjuzmE4x8FWS3B",
                        Balance = 50
                    }
                },
                Eligibilities = new List<EligibilityDto>()
                {
                    new EligibilityDto()
                    {
                        AccountHash = "wcpUPec7pNUKys9pkvPfhjkezekZ99GHpXavbS6M1R4",
                        AssetHash = "BTXVBwuTXWTpPtJC71FPGaeC17NVhu9mS6JavqZqHbYH",
                        IsPrimaryEligible = true,
                        IsSecondaryEligible = false,
                        KycControllerAddress = "CHStDQ5ZFeFW9rbMhw83f7FXg19okxQD9E7"
                    }
                }
            });
        }

        public Result<AssetInfoDto> GetAssetInfo(string assetHash)
        {
            return Result.Success(new AssetInfoDto() {
                Hash = "BTXVBwuTXWTpPtJC71FPGaeC17NVhu9mS6JavqZqHbYH",
                AssetCode = "RET",
                ControllerAddress = "CHLsVaYSPJGFi8BNGd6tP1VvB8UdKbVRDKD",
                ControllerAddresses = new List<ControllerAddressDto>()
                {
                    new ControllerAddressDto()
                    {
                        BlockchainAddress = "CHLsVaYSPJGFi8BNGd6tP1VvB8UdKbVRDKD"
                    },
                    new ControllerAddressDto()
                    {
                        BlockchainAddress = "CHMf4inrS8hnPNEgJVZPRHFhsDPCHSHZfAJ"
                    }
                },
                Holdings = new List<HoldingDto>()
                {
                    new HoldingDto()
                    {
                        AccountHash = "wcpUPec7pNUKys9pkvPfhjkezekZ99GHpXavbS6M1R4",
                        AssetHash = "BTXVBwuTXWTpPtJC71FPGaeC17NVhu9mS6JavqZqHbYH",
                        Balance = 100
                    },
                    new HoldingDto()
                    {
                        AccountHash = "4NZXDMd2uKLTmkKVciu84pkSnzUtic6TKxD61grbGcm9",
                        AssetHash = "BTXVBwuTXWTpPtJC71FPGaeC17NVhu9mS6JavqZqHbYH",
                        Balance = 60
                    }
                },
                Eligibilities = new List<EligibilityDto>()
                {
                    new EligibilityDto()
                    {
                        AccountHash = "wcpUPec7pNUKys9pkvPfhjkezekZ99GHpXavbS6M1R4",
                        AssetHash = "BTXVBwuTXWTpPtJC71FPGaeC17NVhu9mS6JavqZqHbYH",
                        IsPrimaryEligible = true,
                        IsSecondaryEligible = false,
                        KycControllerAddress = "CHStDQ5ZFeFW9rbMhw83f7FXg19okxQD9E7"
                    }
                }
            });
        }

        public Result<ValidatorInfoDto> GetValidatorInfo(string blockchainAddress)
        {
            return Result.Success(new ValidatorInfoDto() {
                BlockchainAddress = "CHMf4inrS8hnPNEgJVZPRHFhsDPCHSHZfAJ",
                NetworkAddress = "localhost:25701",
                SharedRewardPercent = 0,
                IsActive = true
            });
        }

        public Result<IEnumerable<TxInfoShortDto>> GetTxs(int limit, int page)
        {
            return Result.Success(new List<TxInfoShortDto>() {
                new TxInfoShortDto()
                {
                    Hash = "6XTowWarMR1UjzAVfiMYs7hsKK9hPBagR7JtFn7nxgfK",
                    NumberOfActions = 3,
                    SenderAddress = "CHVegEXVwUhK2gbrqnMsYyNSVC7CLTM7qmQ",
                    BlockNumber = 121,
                    Timestamp = DateTime.UtcNow
                },
                new TxInfoShortDto()
                {
                    Hash = "8ZVF1R9vLkV2QGMJxGGffgPqMKv41kemFfVGTzmPfKyg",
                    NumberOfActions = 1,
                    SenderAddress = "CHVegEXVwUhK2gbrqnMsYyNSVC7CLTM7qmQ",
                    BlockNumber = 121,
                }
            }.AsEnumerable());
        }

        public Result<IEnumerable<BlockInfoShortDto>> GetBlocks(int limit, int page)
        {
            return Result.Success(new List<BlockInfoShortDto>() {
                new BlockInfoShortDto() {
                    BlockNumber = 21,
                    Hash = "6pYM6BBGyQttPCTVmpqNpH4i4jE5KjgmaXCWuDJcbiLp",
                    Timestamp = DateTime.UtcNow,
                },
                new BlockInfoShortDto() {
                    BlockNumber = 20,
                    Hash = "EsEYC4f4xvNiSQLRX8P3GNzGeWm1smVvUD5U5oTnDw3b",
                    Timestamp = DateTime.UtcNow.AddSeconds(-10),
                },
            }.AsEnumerable());
        }

        public Result<IEnumerable<ValidatorInfoShortDto>> GetValidators()
        {
            return Result.Success(new List<ValidatorInfoShortDto>() {
                new ValidatorInfoShortDto()
                {
                    BlockchainAddress = "CHMf4inrS8hnPNEgJVZPRHFhsDPCHSHZfAJ",
                    IsActive = true
                },
                new ValidatorInfoShortDto()
                {
                    BlockchainAddress = "CHN5FmdEhjKHynhdbzXxsNB35oxL5195XE5",
                    IsActive = true
                },
                new ValidatorInfoShortDto()
                {
                    BlockchainAddress = "CHStDQ5ZFeFW9rbMhw83f7FXg19okxQD9E7",
                    IsActive = true
                },
                new ValidatorInfoShortDto()
                {
                    BlockchainAddress = "CHVegEXVwUhK2gbrqnMsYyNSVC7CLTM7qmQ",
                    IsActive = true
                },
            }.AsEnumerable());
        }

        public Result<SearchInfoDto> Search(string searchValue)
        {
            throw new NotImplementedException();
        }
    }
}
