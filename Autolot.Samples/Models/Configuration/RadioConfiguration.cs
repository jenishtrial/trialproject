namespace AutoLot.Samples.Models.Configuration;

public class RadioConfiguration : IEntityTypeConfiguration<Radio>
{
    public void Configure(EntityTypeBuilder<Radio> builder)
    {
        builder.HasQueryFilter(r => r.HasTweeters == true);
        builder.HasQueryFilter(r => r.CarNavigation.IsDrivable);
        builder.Property(e => e.CarId).HasColumnName("InventoryId");
        builder.HasIndex(e => e.CarId, "IX_Radios_CarId")
            .IsUnique();
        builder.HasOne(d => d.CarNavigation)
           .WithOne(p => p.RadioNavigation)
           .HasForeignKey<Radio>(d => d.CarId);
    }
}
