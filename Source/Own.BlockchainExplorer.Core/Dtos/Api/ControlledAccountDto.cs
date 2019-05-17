using System.Collections.Generic;

namespace Own.BlockchainExplorer.Core.Dtos.Api
{
    public class ControlledAccountDto
    {
        public string Hash { get; set; }
        public bool IsActive { get; set; }
    }

    public class ControlledAccountDtoEqualityComparer : IEqualityComparer<ControlledAccountDto>
    {
        public bool Equals(ControlledAccountDto x, ControlledAccountDto y)
        {
            return x.Hash == y.Hash;
        }
        public int GetHashCode(ControlledAccountDto obj)
        {
            return obj.Hash.GetHashCode();
        }
    }
}
