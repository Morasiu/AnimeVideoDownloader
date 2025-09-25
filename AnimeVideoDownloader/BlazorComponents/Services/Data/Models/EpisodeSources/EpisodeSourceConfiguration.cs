using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorComponents.Services.Data.Models.EpisodeSources;

public sealed class EpisodeSourceConfiguration : IEntityTypeConfiguration<EpisodeSource>
{
    public void Configure(EntityTypeBuilder<EpisodeSource> builder)
    {
        builder.ToTable("EpisodeSources");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Url).IsRequired().HasMaxLength(1000);
        builder.HasOne(x => x.Episode).WithMany(x => x.Sources).HasForeignKey(x => x.EpisodeId);
        
    }
}