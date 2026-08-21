using Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations;

internal sealed class ResultEntityConfiguration : IEntityTypeConfiguration<ResultEntity>
{
    public void Configure(EntityTypeBuilder<ResultEntity> builder)
    {
        builder.HasKey(result => result.Id);
    }
}
