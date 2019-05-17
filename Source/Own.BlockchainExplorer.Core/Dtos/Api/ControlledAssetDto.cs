using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ControlledAssetDto
    {
        public string Hash { get; set; }
        public string AssetCode { get; set; }
        public bool IsActive { get; set; }
    }

    public class ControlledAssetDtoEqualityComparer : IEqualityComparer<ControlledAssetDto>
    {
        public bool Equals(ControlledAssetDto x, ControlledAssetDto y)
        {
            return x.Hash == y.Hash;
        }
        public int GetHashCode(ControlledAssetDto obj)
        {
            return obj.Hash.GetHashCode();
        }
    }
}
