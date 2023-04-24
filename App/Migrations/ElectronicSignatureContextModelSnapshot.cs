﻿// <auto-generated />
using System;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace App.Migrations
{
    [DbContext(typeof(ElectronicSignatureContext))]
    partial class ElectronicSignatureContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Database.Models.ElectronicSignatureTask", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BestSignContractId")
                        .HasColumnType("text");

                    b.Property<int>("Counter")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Created")
                        .HasPrecision(0)
                        .HasColumnType("timestamp(0) with time zone");

                    b.Property<int>("CurrentStep")
                        .HasColumnType("integer");

                    b.Property<string>("DocuSignEnvelopeId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUpdated")
                        .HasPrecision(0)
                        .HasColumnType("timestamp(0) with time zone");

                    b.HasKey("Id");

                    b.HasIndex("BestSignContractId");

                    b.HasIndex("DocuSignEnvelopeId");

                    b.ToTable("Tasks");
                });

            modelBuilder.Entity("Database.Models.ElectronicSignatureTaskLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasPrecision(0)
                        .HasColumnType("timestamp(0) with time zone");

                    b.Property<DateTime>("LastUpdated")
                        .HasPrecision(0)
                        .HasColumnType("timestamp(0) with time zone");

                    b.Property<string>("Log")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Step")
                        .HasColumnType("integer");

                    b.Property<int>("TaskId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TaskId");

                    b.ToTable("TaskLogs");
                });

            modelBuilder.Entity("Database.Models.TemplateMapping", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("BestSignConfigurationString")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("BestSignTemplateId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Created")
                        .HasPrecision(0)
                        .HasColumnType("timestamp(0) with time zone");

                    b.Property<string>("DocuSignTemplateId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUpdated")
                        .HasPrecision(0)
                        .HasColumnType("timestamp(0) with time zone");

                    b.Property<string>("ParameterMappingsString")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("DocuSignTemplateId");

                    b.ToTable("TemplateMappings");
                });
#pragma warning restore 612, 618
        }
    }
}
