﻿// <auto-generated />
using CarListApp.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CarListApp.API.Migrations
{
    [DbContext(typeof(CarListDbContext))]
    [Migration("20250228154643_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.2");

            modelBuilder.Entity("CarListApp.API.Car", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Make")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Vin")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Cars");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Make = "Honda",
                            Model = "Fit",
                            Vin = "ABC"
                        },
                        new
                        {
                            Id = 2,
                            Make = "Honda",
                            Model = "Civic",
                            Vin = "ABC2"
                        },
                        new
                        {
                            Id = 3,
                            Make = "Honda",
                            Model = "Stream",
                            Vin = "ABC1"
                        },
                        new
                        {
                            Id = 4,
                            Make = "Nissan",
                            Model = "Note",
                            Vin = "ABC4"
                        },
                        new
                        {
                            Id = 5,
                            Make = "Nissan",
                            Model = "Atlas",
                            Vin = "ABC5"
                        },
                        new
                        {
                            Id = 6,
                            Make = "Nissan",
                            Model = "Dualis",
                            Vin = "ABC6"
                        },
                        new
                        {
                            Id = 7,
                            Make = "Nissan",
                            Model = "Murano",
                            Vin = "ABC7"
                        },
                        new
                        {
                            Id = 8,
                            Make = "Audi",
                            Model = "A5",
                            Vin = "ABC8"
                        },
                        new
                        {
                            Id = 9,
                            Make = "BMW",
                            Model = "M3",
                            Vin = "ABC9"
                        },
                        new
                        {
                            Id = 10,
                            Make = "Jaguar",
                            Model = "F-Pace",
                            Vin = "ABC10"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
