using System;
using Microsoft.EntityFrameworkCore;

namespace Own.BlockchainExplorer.Infrastructure.Data.EF
{
    public partial class OwnDb
    {
        public OwnDb(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            this.ConfigureEntities(modelBuilder);
        }

        public override int SaveChanges()
        {
            this.FailIfModificationInfoNotSet();
            return base.SaveChanges();
        }

        private void FailIfModificationInfoNotSet()
        {
            foreach (var entry in this.ChangeTracker.Entries())
            {
                var createDateProperty = entry.Metadata.FindProperty("CreateDate");
                if (createDateProperty != null
                    && entry.CurrentValues.GetValue<DateTime>(createDateProperty) == DateTime.MinValue)
                {
                    throw new Exception($"{entry.Metadata.Name}.{createDateProperty.Name} not set.");
                }

                var createUserProperty = entry.Metadata.FindProperty("CreateUser");
                if (createUserProperty != null
                    && entry.CurrentValues.GetValue<long>(createUserProperty) == 0)
                {
                    throw new Exception($"{entry.Metadata.Name}.{createUserProperty.Name} not set.");
                }

                var modDateProperty = entry.Metadata.FindProperty("ModDate");
                if (modDateProperty != null
                    && entry.CurrentValues.GetValue<DateTime>(modDateProperty) == DateTime.MinValue)
                {
                    throw new Exception($"{entry.Metadata.Name}.{modDateProperty.Name} not set.");
                }

                var modUserProperty = entry.Metadata.FindProperty("ModUser");
                if (modUserProperty != null
                    && entry.CurrentValues.GetValue<long>(modUserProperty) == 0)
                {
                    throw new Exception($"{entry.Metadata.Name}.{modUserProperty.Name} not set.");
                }
            }
        }
    }
}
