using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuakeNote.Auth.Data.Entities;
using QuakeNote.Shared.Contracts;

namespace QuakeNote.Auth.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", t => t.HasCheckConstraint("ck_users_role", "role IN ('user','admin')"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Uid).HasDefaultValueSql("gen_random_uuid()");
        builder.HasIndex(x => x.Uid).IsUnique();

        builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.Property(x => x.Role).HasMaxLength(16).IsRequired().HasDefaultValue(Roles.User);
        builder.Property(x => x.BirthDate).HasColumnType("date");
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("now()");

        builder.HasOne(x => x.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Settings)
            .WithOne(s => s.User)
            .HasForeignKey<UserSettings>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(20);
        builder.Property(x => x.LastName).HasMaxLength(30);
        builder.Property(x => x.Location).HasMaxLength(50);
    }
}

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.ToTable("user_settings",
            t => t.HasCheckConstraint("ck_user_settings_min_magnitude", "min_magnitude BETWEEN 1 AND 10"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DefaultRegion).HasMaxLength(50);
    }
}

public class AuthCodeConfiguration : IEntityTypeConfiguration<AuthCode>
{
    public void Configure(EntityTypeBuilder<AuthCode> builder)
    {
        builder.ToTable("auth_codes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
        builder.HasIndex(x => x.Email);
        builder.Property(x => x.CodeHash).HasMaxLength(255).IsRequired();
    }
}