using Hotel.Application.Domain;
using Hotel.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Hotel.Infrastructure.Migrations
{
    [DbContext(typeof(HotelDbContext))]
    partial class HotelDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("Hotel.Application.Domain.Guest", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasMaxLength(128)
                    .HasColumnType("TEXT");

                b.Property<string>("FirstName")
                    .IsRequired()
                    .HasMaxLength(64)
                    .HasColumnType("TEXT");

                b.Property<string>("IdentityDocument")
                    .HasMaxLength(64)
                    .HasColumnType("TEXT");

                b.Property<string>("LastName")
                    .IsRequired()
                    .HasMaxLength(64)
                    .HasColumnType("TEXT");

                b.Property<string>("Phone")
                    .HasMaxLength(32)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("Email");

                b.ToTable("Guests");

                b.HasData(
                    new Guest { Id = 1, FirstName = "Jan", LastName = "Kowalski", Email = "jan.kowalski@example.com", Phone = "500600700", IdentityDocument = "ABC123456" },
                    new Guest { Id = 2, FirstName = "Anna", LastName = "Nowak", Email = "anna.nowak@example.com", Phone = null, IdentityDocument = null },
                    new Guest { Id = 3, FirstName = "Piotr", LastName = "Zielinski", Email = "piotr.zielinski@example.com", Phone = "123123123", IdentityDocument = "XYZ987654" }
                );
            });

            modelBuilder.Entity("Hotel.Application.Domain.Room", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<int>("Capacity")
                    .HasColumnType("INTEGER");

                b.Property<bool>("IsActive")
                    .HasColumnType("INTEGER");

                b.Property<string>("Number")
                    .IsRequired()
                    .HasMaxLength(32)
                    .HasColumnType("TEXT");

                b.Property<decimal>("PricePerNight")
                    .HasColumnType("TEXT");

                b.Property<string>("Type")
                    .IsRequired()
                    .HasMaxLength(32)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("Number")
                    .IsUnique();

                b.ToTable("Rooms");

                b.HasData(
                    new Room { Id = 1, Number = "101", Type = "Standard", Capacity = 2, PricePerNight = 250m, IsActive = true },
                    new Room { Id = 2, Number = "102", Type = "Standard", Capacity = 3, PricePerNight = 320m, IsActive = true },
                    new Room { Id = 3, Number = "201", Type = "Deluxe", Capacity = 2, PricePerNight = 450m, IsActive = true },
                    new Room { Id = 4, Number = "202", Type = "Deluxe", Capacity = 4, PricePerNight = 600m, IsActive = true },
                    new Room { Id = 5, Number = "301", Type = "Suite", Capacity = 4, PricePerNight = 900m, IsActive = true },
                    new Room { Id = 6, Number = "999", Type = "Maintenance", Capacity = 1, PricePerNight = 0m, IsActive = false }
                );
            });

            var dateOnlyConverter = new ValueConverter<DateOnly, string>(
                d => d.ToString("yyyy-MM-dd"),
                s => DateOnly.Parse(s));

            modelBuilder.Entity("Hotel.Application.Domain.Reservation", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("INTEGER");

                b.Property<DateOnly>("CheckIn")
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasConversion(dateOnlyConverter)
                    .HasColumnType("TEXT");

                b.Property<DateOnly>("CheckOut")
                    .IsRequired()
                    .HasMaxLength(10)
                    .HasConversion(dateOnlyConverter)
                    .HasColumnType("TEXT");

                b.Property<int>("GuestId")
                    .HasColumnType("INTEGER");

                b.Property<int>("GuestsCount")
                    .HasColumnType("INTEGER");

                b.Property<int>("RoomId")
                    .HasColumnType("INTEGER");

                b.Property<byte[]>("RowVersion")
                    .IsRowVersion()
                    .HasColumnType("BLOB");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasMaxLength(16)
                    .HasColumnType("TEXT");

                b.Property<decimal>("TotalPrice")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("GuestId");

                b.HasIndex("RoomId", "CheckIn", "CheckOut");

                b.ToTable("Reservations");

                b.HasOne("Hotel.Application.Domain.Guest", "Guest")
                    .WithMany("Reservations")
                    .HasForeignKey("GuestId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("Hotel.Application.Domain.Room", "Room")
                    .WithMany("Reservations")
                    .HasForeignKey("RoomId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Guest");
                b.Navigation("Room");

                b.HasData(
                    new { Id = 1, RoomId = 1, GuestId = 1, CheckIn = "2030-01-10", CheckOut = "2030-01-12", GuestsCount = 2, TotalPrice = 500m, Status = "Active", RowVersion = new byte[0] }
                );
            });
#pragma warning restore 612, 618
        }
    }
}
