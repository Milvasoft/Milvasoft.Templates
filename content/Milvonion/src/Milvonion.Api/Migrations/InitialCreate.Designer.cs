﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Milvonion.Domain.JsonModels;
using Milvonion.Infrastructure.Persistence.Context;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Milvonion.Api.Migrations
{
    [DbContext(typeof(MilvonionDbContext))]
    [Migration("20240914184711_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Milvonion.Domain.ActivityLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<byte>("Activity")
                        .HasColumnType("smallint");

                    b.Property<DateTimeOffset>("ActivityDate")
                        .HasMaxLength(255)
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.ToTable("ActivityLogs");
                });

            modelBuilder.Entity("Milvonion.Domain.ApiLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<long>("ElapsedMs")
                        .HasColumnType("bigint");

                    b.Property<string>("IpAddress")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<RequestInfo>("RequestInfoJson")
                        .HasColumnType("jsonb");

                    b.Property<ResponseInfo>("ResponseInfoJson")
                        .HasColumnType("jsonb");

                    b.Property<string>("Severity")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<DateTimeOffset>("Timestamp")
                        .HasMaxLength(255)
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("TransactionId")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("UserName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.ToTable("ApiLogs");
                });

            modelBuilder.Entity("Milvonion.Domain.MethodLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("CacheInfo")
                        .HasColumnType("text");

                    b.Property<string>("ClassName")
                        .HasColumnType("text");

                    b.Property<int>("ElapsedMs")
                        .HasColumnType("integer");

                    b.Property<string>("Exception")
                        .HasColumnType("text");

                    b.Property<bool>("IsSuccess")
                        .HasColumnType("boolean");

                    b.Property<string>("MethodName")
                        .HasColumnType("text");

                    b.Property<string>("MethodParams")
                        .HasColumnType("text");

                    b.Property<string>("MethodResult")
                        .HasColumnType("text");

                    b.Property<string>("Namespace")
                        .HasColumnType("text");

                    b.Property<string>("TransactionId")
                        .HasColumnType("text");

                    b.Property<DateTime>("UtcLogTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("MethodLogs");
                });

            modelBuilder.Entity("Milvonion.Domain.MigrationHistory", b =>
                {
                    b.Property<string>("MigrationId")
                        .HasColumnType("text");

                    b.Property<bool>("MigrationCompleted")
                        .HasColumnType("boolean");

                    b.Property<string>("ProductVersion")
                        .HasColumnType("text");

                    b.HasKey("MigrationId");

                    b.ToTable("_MigrationHistory");
                });

            modelBuilder.Entity("Milvonion.Domain.Permission", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("NormalizedName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("PermissionGroup")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("PermissionGroupDescription")
                        .HasMaxLength(255)
                        .HasColumnType("character varying(255)");

                    b.HasKey("Id");

                    b.ToTable("Permissions");
                });

            modelBuilder.Entity("Milvonion.Domain.Role", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorUserName")
                        .HasColumnType("text");

                    b.Property<string>("DeleterUserName")
                        .HasColumnType("text");

                    b.Property<DateTime?>("DeletionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastModificationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastModifierUserName")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Milvonion.Domain.RolePermissionRelation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("PermissionId")
                        .HasColumnType("integer");

                    b.Property<int>("RoleId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("PermissionId");

                    b.HasIndex("RoleId");

                    b.ToTable("RolePermissionRelations");
                });

            modelBuilder.Entity("Milvonion.Domain.UI.MenuGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorUserName")
                        .HasColumnType("text");

                    b.Property<List<MenuGroupTranslation>>("Translations")
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.ToTable("MenuGroups");
                });

            modelBuilder.Entity("Milvonion.Domain.UI.MenuItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorUserName")
                        .HasColumnType("text");

                    b.Property<int>("GroupId")
                        .HasColumnType("integer");

                    b.Property<string>("PageName")
                        .HasColumnType("text");

                    b.Property<int?>("ParentId")
                        .HasColumnType("integer");

                    b.Property<List<string>>("PermissionOrGroupNames")
                        .HasColumnType("jsonb");

                    b.Property<List<MenuItemTranslation>>("Translations")
                        .HasColumnType("jsonb");

                    b.Property<string>("Url")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("GroupId");

                    b.HasIndex("ParentId");

                    b.ToTable("MenuItems");
                });

            modelBuilder.Entity("Milvonion.Domain.UI.Page", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<List<string>>("CreatePermissions")
                        .HasColumnType("jsonb");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorUserName")
                        .HasColumnType("text");

                    b.Property<List<string>>("DeletePermissions")
                        .HasColumnType("jsonb");

                    b.Property<List<string>>("DetailPermissions")
                        .HasColumnType("jsonb");

                    b.Property<List<string>>("EditPermissions")
                        .HasColumnType("jsonb");

                    b.Property<bool>("HasCreate")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasDelete")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasDetail")
                        .HasColumnType("boolean");

                    b.Property<bool>("HasEdit")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Pages");
                });

            modelBuilder.Entity("Milvonion.Domain.UI.PageAction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("ActionName")
                        .HasColumnType("text");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorUserName")
                        .HasColumnType("text");

                    b.Property<int>("PageId")
                        .HasColumnType("integer");

                    b.Property<List<string>>("Permissions")
                        .HasColumnType("jsonb");

                    b.Property<List<PageActionTranslation>>("Translations")
                        .HasColumnType("jsonb");

                    b.HasKey("Id");

                    b.HasIndex("PageId");

                    b.ToTable("PageActions");
                });

            modelBuilder.Entity("Milvonion.Domain.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorUserName")
                        .HasColumnType("text");

                    b.Property<string>("DeleterUserName")
                        .HasColumnType("text");

                    b.Property<DateTime?>("DeletionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Email")
                        .HasColumnType("text");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastModificationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastModifierUserName")
                        .HasColumnType("text");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("boolean");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("text");

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("text");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("text");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("boolean");

                    b.Property<string>("Surname")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("boolean");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.Property<byte>("UserType")
                        .HasColumnType("smallint");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Milvonion.Domain.UserRoleRelation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("RoleId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("UserRoleRelations");
                });

            modelBuilder.Entity("Milvonion.Domain.UserSession", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("AccessToken")
                        .HasColumnType("text");

                    b.Property<DateTime?>("CreationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CreatorUserName")
                        .HasColumnType("text");

                    b.Property<string>("DeleterUserName")
                        .HasColumnType("text");

                    b.Property<DateTime?>("DeletionDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("DeviceId")
                        .HasColumnType("text");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("boolean");

                    b.Property<DateTime?>("LastModificationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("LastModifierUserName")
                        .HasColumnType("text");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.Property<string>("UserName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserSessions");
                });

            modelBuilder.Entity("Milvonion.Domain.RolePermissionRelation", b =>
                {
                    b.HasOne("Milvonion.Domain.Permission", "Permission")
                        .WithMany("RolePermissionRelations")
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Milvonion.Domain.Role", "Role")
                        .WithMany("RolePermissionRelations")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Permission");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("Milvonion.Domain.UI.MenuItem", b =>
                {
                    b.HasOne("Milvonion.Domain.UI.MenuGroup", "Group")
                        .WithMany("MenuItems")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Milvonion.Domain.UI.MenuItem", "Parent")
                        .WithMany("Childrens")
                        .HasForeignKey("ParentId");

                    b.Navigation("Group");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("Milvonion.Domain.UI.PageAction", b =>
                {
                    b.HasOne("Milvonion.Domain.UI.Page", "Page")
                        .WithMany("AdditionalActions")
                        .HasForeignKey("PageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Page");
                });

            modelBuilder.Entity("Milvonion.Domain.UserRoleRelation", b =>
                {
                    b.HasOne("Milvonion.Domain.Role", "Role")
                        .WithMany("UserRoleRelations")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Milvonion.Domain.User", "User")
                        .WithMany("RoleRelations")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Milvonion.Domain.UserSession", b =>
                {
                    b.HasOne("Milvonion.Domain.User", "User")
                        .WithMany("Sessions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Milvonion.Domain.Permission", b =>
                {
                    b.Navigation("RolePermissionRelations");
                });

            modelBuilder.Entity("Milvonion.Domain.Role", b =>
                {
                    b.Navigation("RolePermissionRelations");

                    b.Navigation("UserRoleRelations");
                });

            modelBuilder.Entity("Milvonion.Domain.UI.MenuGroup", b =>
                {
                    b.Navigation("MenuItems");
                });

            modelBuilder.Entity("Milvonion.Domain.UI.MenuItem", b =>
                {
                    b.Navigation("Childrens");
                });

            modelBuilder.Entity("Milvonion.Domain.UI.Page", b =>
                {
                    b.Navigation("AdditionalActions");
                });

            modelBuilder.Entity("Milvonion.Domain.User", b =>
                {
                    b.Navigation("RoleRelations");

                    b.Navigation("Sessions");
                });
#pragma warning restore 612, 618
        }
    }
}
