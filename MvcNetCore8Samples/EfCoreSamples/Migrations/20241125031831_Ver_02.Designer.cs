﻿// <auto-generated />
using System;
using EfCoreSamples.Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace EfCoreSamples.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241125031831_Ver_02")]
    partial class Ver_02
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.7");

            modelBuilder.Entity("EfCoreSamples.Domains.ChildItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<int>("ParentId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RowVersion")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("ChildItems");
                });

            modelBuilder.Entity("EfCoreSamples.Domains.FileData", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ContentType")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Data")
                        .HasColumnType("BLOB");

                    b.Property<string>("FileName")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("FileDatas", (string)null);
                });

            modelBuilder.Entity("EfCoreSamples.Domains.ParentItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ModifiedDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("TEXT");

                    b.Property<int>("RowVersion")
                        .IsConcurrencyToken()
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("ParentItems");
                });

            modelBuilder.Entity("EfCoreSamples.Domains.ChildItem", b =>
                {
                    b.HasOne("EfCoreSamples.Domains.ParentItem", "Parent")
                        .WithMany("ChildItems")
                        .HasForeignKey("ParentId")
                        .IsRequired()
                        .HasConstraintName("FK_ChildItems_ParentItems");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("EfCoreSamples.Domains.ParentItem", b =>
                {
                    b.Navigation("ChildItems");
                });
#pragma warning restore 612, 618
        }
    }
}