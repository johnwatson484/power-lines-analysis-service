﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PowerLinesAnalysisService.Data;

namespace PowerLinesAnalysisService.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20200323202803_AddResultCreated")]
    partial class AddResultCreated
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("PowerLinesAnalysisService.Models.Result", b =>
                {
                    b.Property<int>("ResultId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("resultId")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("AwayOddsAverage")
                        .HasColumnName("awayOddsAverage")
                        .HasColumnType("numeric");

                    b.Property<string>("AwayTeam")
                        .HasColumnName("awayTeam")
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasColumnName("created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("Date")
                        .HasColumnName("date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Division")
                        .HasColumnName("division")
                        .HasColumnType("text");

                    b.Property<decimal>("DrawOddsAverage")
                        .HasColumnName("drawOddsAverage")
                        .HasColumnType("numeric");

                    b.Property<int>("FullTimeAwayGoals")
                        .HasColumnName("fullTimeAwayGoals")
                        .HasColumnType("integer");

                    b.Property<int>("FullTimeHomeGoals")
                        .HasColumnName("fullTimeHomeGoals")
                        .HasColumnType("integer");

                    b.Property<string>("FullTimeResult")
                        .HasColumnName("fullTimeResult")
                        .HasColumnType("text");

                    b.Property<int>("HalfTimeAwayGoals")
                        .HasColumnName("halfTimeAwayGoals")
                        .HasColumnType("integer");

                    b.Property<int>("HalfTimeHomeGoals")
                        .HasColumnName("halfTimeHomeGoals")
                        .HasColumnType("integer");

                    b.Property<string>("HalfTimeResult")
                        .HasColumnName("halfTimeResult")
                        .HasColumnType("text");

                    b.Property<decimal>("HomeOddsAverage")
                        .HasColumnName("homeOddsAverage")
                        .HasColumnType("numeric");

                    b.Property<string>("HomeTeam")
                        .HasColumnName("homeTeam")
                        .HasColumnType("text");

                    b.HasKey("ResultId");

                    b.HasIndex("Date", "HomeTeam", "AwayTeam")
                        .IsUnique();

                    b.ToTable("results");
                });
#pragma warning restore 612, 618
        }
    }
}