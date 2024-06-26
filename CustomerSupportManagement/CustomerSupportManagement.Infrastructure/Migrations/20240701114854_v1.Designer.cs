﻿// <auto-generated />
using System;
using CustomerSupportManagement.Infrastructure.SQLRepo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CustomerSupportManagement.Infrastructure.Migrations
{
    [DbContext(typeof(SQLDbContext))]
    [Migration("20240701114854_v1")]
    partial class v1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("CustomerSupportManagement.Domain.Entities.SupportAgent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("SupportAgents");
                });

            modelBuilder.Entity("CustomerSupportManagement.Domain.Entities.SupportTicket", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<string>("customerEmail")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<Guid>("customerId")
                        .HasColumnType("char(36)");

                    b.Property<string>("customerName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<bool>("status")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("subject")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<Guid?>("supportAgentId")
                        .HasColumnType("char(36)");

                    b.HasKey("Id");

                    b.HasIndex("supportAgentId");

                    b.ToTable("SupportTickets");
                });

            modelBuilder.Entity("CustomerSupportManagement.Domain.Entities.SupportTicket", b =>
                {
                    b.HasOne("CustomerSupportManagement.Domain.Entities.SupportAgent", "supportAgent")
                        .WithMany("SupportTickets")
                        .HasForeignKey("supportAgentId");

                    b.Navigation("supportAgent");
                });

            modelBuilder.Entity("CustomerSupportManagement.Domain.Entities.SupportAgent", b =>
                {
                    b.Navigation("SupportTickets");
                });
#pragma warning restore 612, 618
        }
    }
}
