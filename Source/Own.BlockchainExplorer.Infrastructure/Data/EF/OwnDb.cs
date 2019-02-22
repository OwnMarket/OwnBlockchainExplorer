////////////////////////////////////////////////////////////////////////////////////////////////////
// THIS CODE IS GENERATED - DO NOT CHANGE IT MANUALLY!
////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Own.BlockchainExplorer.Core.Models;

namespace Own.BlockchainExplorer.Infrastructure.Data.EF
{
    public partial class OwnDb : DbContext
    {
        public virtual DbSet<Address> Addresses { get; set; }

        private void ConfigureEntities(ModelBuilder modelBuilder)
        {
            // Address
            var address = modelBuilder.Entity<Address>()
                .ToTable("address", "own");
            address.HasKey(c => c.AddressId);
            address.Property(e => e.AddressId)
                .HasColumnName("address_id")
                .IsRequired()
                .ValueGeneratedOnAdd();
            address.Property(e => e.BlockchainAddress)
                .HasColumnName("blockchain_address")
                .IsRequired();
            address.Property(e => e.ChxBalance)
                .HasColumnName("chx_balance")
                .IsRequired();
            address.Property(e => e.Nonce)
                .HasColumnName("nonce")
                .IsRequired();
        }
    }
}