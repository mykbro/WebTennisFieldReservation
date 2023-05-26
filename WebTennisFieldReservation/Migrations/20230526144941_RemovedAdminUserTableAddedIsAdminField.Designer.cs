﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebTennisFieldReservation.Data;

#nullable disable

namespace WebTennisFieldReservation.Migrations
{
    [DbContext(typeof(CourtComplexDbContext))]
    [Migration("20230526144941_RemovedAdminUserTableAddedIsAdminField")]
    partial class RemovedAdminUserTableAddedIsAdminField
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WebTennisFieldReservation.Entities.Court", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Courts");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.Reservation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("LastUpdateOn")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("PaymentConfirmationToken")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PaymentId")
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("decimal(18,2)");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Reservations");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.ReservationEntry", b =>
                {
                    b.Property<Guid>("ReservationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("EntryNr")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("ReservationSlotId")
                        .HasColumnType("int");

                    b.HasKey("ReservationId", "EntryNr");

                    b.HasIndex("ReservationSlotId");

                    b.ToTable("ReservationEntries");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.ReservationSlot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CourtId")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("DaySlot")
                        .HasColumnType("int");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("bit");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("CourtId", "Date", "DaySlot")
                        .IsUnique();

                    b.ToTable("ReservationsSlots");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.Template", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.TemplateEntry", b =>
                {
                    b.Property<int>("TemplateId")
                        .HasColumnType("int");

                    b.Property<int>("WeekSlot")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("TemplateId", "WeekSlot");

                    b.ToTable("TemplateEntries");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Address")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<DateTime>("BirthDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<int>("Pbkdf2Iterations")
                        .HasColumnType("int");

                    b.Property<byte[]>("PwdHash")
                        .IsRequired()
                        .HasColumnType("binary(32)");

                    b.Property<byte[]>("PwdSalt")
                        .IsRequired()
                        .HasColumnType("binary(32)");

                    b.Property<DateTimeOffset>("RegistrationTimestamp")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("SecurityStamp")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.Reservation", b =>
                {
                    b.HasOne("WebTennisFieldReservation.Entities.User", "User")
                        .WithMany("Reservations")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.ReservationEntry", b =>
                {
                    b.HasOne("WebTennisFieldReservation.Entities.Reservation", "Reservation")
                        .WithMany("ReservationEntries")
                        .HasForeignKey("ReservationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebTennisFieldReservation.Entities.ReservationSlot", "ReservationSlot")
                        .WithMany()
                        .HasForeignKey("ReservationSlotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Reservation");

                    b.Navigation("ReservationSlot");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.ReservationSlot", b =>
                {
                    b.HasOne("WebTennisFieldReservation.Entities.Court", "Court")
                        .WithMany("ReservationSlots")
                        .HasForeignKey("CourtId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Court");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.TemplateEntry", b =>
                {
                    b.HasOne("WebTennisFieldReservation.Entities.Template", "Template")
                        .WithMany("TemplateEntries")
                        .HasForeignKey("TemplateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Template");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.Court", b =>
                {
                    b.Navigation("ReservationSlots");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.Reservation", b =>
                {
                    b.Navigation("ReservationEntries");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.Template", b =>
                {
                    b.Navigation("TemplateEntries");
                });

            modelBuilder.Entity("WebTennisFieldReservation.Entities.User", b =>
                {
                    b.Navigation("Reservations");
                });
#pragma warning restore 612, 618
        }
    }
}
