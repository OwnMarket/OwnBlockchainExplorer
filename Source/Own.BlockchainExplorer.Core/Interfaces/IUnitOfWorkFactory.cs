using Own.BlockchainExplorer.Core.Enums;

namespace Own.BlockchainExplorer.Core.Interfaces
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create(UnitOfWorkMode mode = UnitOfWorkMode.ReadOnly);
    }
}
