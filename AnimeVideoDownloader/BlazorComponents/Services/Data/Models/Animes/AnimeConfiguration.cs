using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorComponents.Services.Data.Models.Animes;

public sealed class AnimeConfiguration : IEntityTypeConfiguration<Anime>
{
    public void Configure(EntityTypeBuilder<Anime> builder)
    {
        builder.ToTable("Animes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(250);
        builder.Property(x => x.SourceUrl).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.Directory).IsRequired().HasMaxLength(1000);
    }
}