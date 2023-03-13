﻿// <auto-generated />
using System;
using HuppyService.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace HuppyService.Infrastructure.Migrations
{
    [DbContext(typeof(HuppyDbContext))]
    [Migration("20221006172417_init")]
    partial class init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.9");

            modelBuilder.Entity("HuppyService.Core.Models.CommandLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ChannelId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CommandName")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("ErrorMessage")
                        .HasColumnType("TEXT");

                    b.Property<long>("ExecutionTimeMs")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("GuildId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsSuccess")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("CommandLogs");
                });

            modelBuilder.Entity("HuppyService.Core.Models.Reminder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("RemindDate")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("HuppyService.Core.Models.Server", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("GreetMessage")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("RoleID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ServerName")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("ServerRoomsId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("UseGreet")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("HuppyService.Core.Models.ServerRooms", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GreetingRoom")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("OutputRoom")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("ServerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ServerRooms");
                });

            modelBuilder.Entity("HuppyService.Core.Models.Ticket", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ClosedDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsClosed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TicketAnswer")
                        .HasColumnType("TEXT");

                    b.Property<string>("Topic")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("Tickets");
                });

            modelBuilder.Entity("HuppyService.Core.Models.CommandLog", b =>
                {
                    b.HasOne("HuppyService.Core.Models.Server", "Guild")
                        .WithMany("CommangLogs")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("HuppyService.Core.Models.ServerRooms", b =>
                {
                    b.HasOne("HuppyService.Core.Models.Server", "Server")
                        .WithOne("Rooms")
                        .HasForeignKey("HuppyService.Core.Models.ServerRooms", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Server");
                });

            modelBuilder.Entity("HuppyService.Core.Models.Server", b =>
                {
                    b.Navigation("CommangLogs");

                    b.Navigation("Rooms");
                });
#pragma warning restore 612, 618
        }
    }
}
