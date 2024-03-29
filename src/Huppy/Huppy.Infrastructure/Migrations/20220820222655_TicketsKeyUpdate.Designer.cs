﻿// <auto-generated />
using System;
using Huppy.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Huppy.Infrastructure.Migrations
{
    [DbContext(typeof(HuppyDbContext))]
    [Migration("20220820222655_TicketsKeyUpdate")]
    partial class TicketsKeyUpdate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.2");

            modelBuilder.Entity("Huppy.Core.Models.CommandLog", b =>
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

                    b.HasIndex("UserId");

                    b.ToTable("CommandLogs");
                });

            modelBuilder.Entity("Huppy.Core.Models.Reminder", b =>
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

                    b.HasIndex("UserId");

                    b.ToTable("Reminders");
                });

            modelBuilder.Entity("Huppy.Core.Models.Server", b =>
                {
                    b.Property<ulong>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("GreetMessage")
                        .HasColumnType("TEXT");

                    b.Property<ulong>("RoleID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ServerName")
                        .HasColumnType("TEXT");

                    b.Property<ulong?>("ServerRoomsID")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("UseGreet")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("Huppy.Core.Models.ServerRooms", b =>
                {
                    b.Property<ulong>("ServerRoomsID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("GreetingRoom")
                        .HasColumnType("INTEGER");

                    b.Property<ulong>("OutputRoom")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("ServerID")
                        .HasColumnType("INTEGER");

                    b.HasKey("ServerRoomsID");

                    b.ToTable("ServerRooms");
                });

            modelBuilder.Entity("Huppy.Core.Models.Ticket", b =>
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

                    b.Property<ulong>("UserId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Tickets");
                });

            modelBuilder.Entity("Huppy.Core.Models.User", b =>
                {
                    b.Property<ulong>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("JoinDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Username")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Huppy.Core.Models.CommandLog", b =>
                {
                    b.HasOne("Huppy.Core.Models.Server", "Guild")
                        .WithMany()
                        .HasForeignKey("GuildId");

                    b.HasOne("Huppy.Core.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Guild");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Huppy.Core.Models.Reminder", b =>
                {
                    b.HasOne("Huppy.Core.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Huppy.Core.Models.ServerRooms", b =>
                {
                    b.HasOne("Huppy.Core.Models.Server", "Server")
                        .WithOne("Rooms")
                        .HasForeignKey("Huppy.Core.Models.ServerRooms", "ServerRoomsID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Server");
                });

            modelBuilder.Entity("Huppy.Core.Models.Ticket", b =>
                {
                    b.HasOne("Huppy.Core.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Huppy.Core.Models.Server", b =>
                {
                    b.Navigation("Rooms");
                });
#pragma warning restore 612, 618
        }
    }
}
