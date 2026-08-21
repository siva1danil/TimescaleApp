using Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

internal sealed class ValueEntityConfiguration : IEntityTypeConfiguration<ValueEntity>
{
    public void Configure(EntityTypeBuilder<ValueEntity> builder)
    {
        builder.HasKey(value => value.Id);

        builder.HasOne(value => value.Result)
            .WithMany(result => result.Values)
            .HasForeignKey(value => value.ResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
