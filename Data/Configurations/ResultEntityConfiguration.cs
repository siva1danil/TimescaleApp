using Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

internal sealed class ResultEntityConfiguration : IEntityTypeConfiguration<ResultEntity>
{
    public void Configure(EntityTypeBuilder<ResultEntity> builder)
    {
        builder.HasKey(result => result.Id);

        builder.HasIndex(result => result.Filename)
            .IsUnique();

        builder.HasIndex(result => result.FirstOperationDate);

        builder.HasIndex(result => result.AverageValue);

        builder.HasIndex(result => result.AverageExecutionTime);
    }
}
