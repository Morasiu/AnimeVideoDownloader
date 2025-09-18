using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorComponents.Services.Data.Models.Episodes;

public sealed class EpisodeConfiguration : IEntityTypeConfiguration<Episode>
{
    public void Configure(EntityTypeBuilder<Episode> builder)
    {
        builder.ToTable("Episodes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(250);
        
    }
}