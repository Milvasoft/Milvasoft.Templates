using Microsoft.EntityFrameworkCore.Migrations;
using Milvonion.Domain;
using Milvonion.Domain.JsonModels;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Milvonion.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<bool>(name: nameof(MigrationHistory.MigrationCompleted),
                                             schema: "public",
                                             table: TableNames.MigrationHistory,
                                             defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Activity = table.Column<byte>(type: "smallint", nullable: false),
                    ActivityDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransactionId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", maxLength: 255, nullable: false),
                    Path = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RequestInfoJson = table.Column<RequestInfo>(type: "jsonb", nullable: true),
                    ResponseInfoJson = table.Column<ResponseInfo>(type: "jsonb", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ElapsedMs = table.Column<long>(type: "bigint", nullable: false),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Exception = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Translations = table.Column<List<MenuGroupTranslation>>(type: "jsonb", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatorUserName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuGroups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MethodLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransactionId = table.Column<string>(type: "text", nullable: true),
                    Namespace = table.Column<string>(type: "text", nullable: true),
                    ClassName = table.Column<string>(type: "text", nullable: true),
                    MethodName = table.Column<string>(type: "text", nullable: true),
                    MethodParams = table.Column<string>(type: "text", nullable: true),
                    MethodResult = table.Column<string>(type: "text", nullable: true),
                    ElapsedMs = table.Column<int>(type: "integer", nullable: false),
                    UtcLogTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CacheInfo = table.Column<string>(type: "text", nullable: true),
                    Exception = table.Column<string>(type: "text", nullable: true),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MethodLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    HasCreate = table.Column<bool>(type: "boolean", nullable: false),
                    HasDetail = table.Column<bool>(type: "boolean", nullable: false),
                    HasEdit = table.Column<bool>(type: "boolean", nullable: false),
                    HasDelete = table.Column<bool>(type: "boolean", nullable: false),
                    CreatePermissions = table.Column<List<string>>(type: "jsonb", nullable: true),
                    DetailPermissions = table.Column<List<string>>(type: "jsonb", nullable: true),
                    EditPermissions = table.Column<List<string>>(type: "jsonb", nullable: true),
                    DeletePermissions = table.Column<List<string>>(type: "jsonb", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatorUserName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PermissionGroup = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PermissionGroupDescription = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatorUserName = table.Column<string>(type: "text", nullable: true),
                    LastModificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserName = table.Column<string>(type: "text", nullable: true),
                    DeletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleterUserName = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Surname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UserType = table.Column<byte>(type: "smallint", nullable: false),
                    LastModificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatorUserName = table.Column<string>(type: "text", nullable: true),
                    LastModifierUserName = table.Column<string>(type: "text", nullable: true),
                    DeleterUserName = table.Column<string>(type: "text", nullable: true),
                    DeletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Url = table.Column<string>(type: "text", nullable: true),
                    PageName = table.Column<string>(type: "text", nullable: true),
                    PermissionOrGroupNames = table.Column<List<string>>(type: "jsonb", nullable: true),
                    Translations = table.Column<List<MenuItemTranslation>>(type: "jsonb", nullable: true),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    ParentId = table.Column<int>(type: "integer", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatorUserName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_MenuGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "MenuGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenuItems_MenuItems_ParentId",
                        column: x => x.ParentId,
                        principalTable: "MenuItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PageActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActionName = table.Column<string>(type: "text", nullable: true),
                    Permissions = table.Column<List<string>>(type: "jsonb", nullable: true),
                    Translations = table.Column<List<PageActionTranslation>>(type: "jsonb", nullable: true),
                    PageId = table.Column<int>(type: "integer", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatorUserName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PageActions_Pages_PageId",
                        column: x => x.PageId,
                        principalTable: "Pages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissionRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    PermissionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissionRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissionRelations_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissionRelations_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoleRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoleRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRoleRelations_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoleRelations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    AccessToken = table.Column<string>(type: "text", nullable: true),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeviceId = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatorUserName = table.Column<string>(type: "text", nullable: true),
                    LastModificationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifierUserName = table.Column<string>(type: "text", nullable: true),
                    DeletionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleterUserName = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityLogs_ActivityDate",
                table: "ActivityLogs",
                column: "ActivityDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_Path",
                table: "ApiLogs",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_Severity",
                table: "ApiLogs",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_Timestamp",
                table: "ApiLogs",
                column: "Timestamp",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_TransactionId",
                table: "ApiLogs",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ApiLogs_UserName",
                table: "ApiLogs",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_MenuGroups_CreationDate",
                table: "MenuGroups",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_CreationDate",
                table: "MenuItems",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_GroupId",
                table: "MenuItems",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ParentId",
                table: "MenuItems",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodLogs_IsSuccess",
                table: "MethodLogs",
                column: "IsSuccess");

            migrationBuilder.CreateIndex(
                name: "IX_MethodLogs_MethodName",
                table: "MethodLogs",
                column: "MethodName");

            migrationBuilder.CreateIndex(
                name: "IX_MethodLogs_TransactionId",
                table: "MethodLogs",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_MethodLogs_UtcLogTime",
                table: "MethodLogs",
                column: "UtcLogTime",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_PageActions_CreationDate",
                table: "PageActions",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_PageActions_PageId",
                table: "PageActions",
                column: "PageId");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_CreationDate",
                table: "Pages",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Pages_Name",
                table: "Pages",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionGroup_Name",
                table: "Permissions",
                columns: new[] { "PermissionGroup", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionRelations_PermissionId",
                table: "RolePermissionRelations",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissionRelations_RoleId",
                table: "RolePermissionRelations",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreationDate",
                table: "Roles",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_IsDeleted",
                table: "Roles",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleRelations_RoleId",
                table: "UserRoleRelations",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoleRelations_UserId",
                table: "UserRoleRelations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreationDate",
                table: "Users",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsDeleted",
                table: "Users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserName",
                table: "Users",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_CreationDate",
                table: "UserSessions",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_IsDeleted",
                table: "UserSessions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                table: "UserSessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserName_DeviceId",
                table: "UserSessions",
                columns: new[] { "UserName", "DeviceId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_MigrationHistory");

            migrationBuilder.DropTable(
                name: "ActivityLogs");

            migrationBuilder.DropTable(
                name: "ApiLogs");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "MethodLogs");

            migrationBuilder.DropTable(
                name: "PageActions");

            migrationBuilder.DropTable(
                name: "RolePermissionRelations");

            migrationBuilder.DropTable(
                name: "UserRoleRelations");

            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "MenuGroups");

            migrationBuilder.DropTable(
                name: "Pages");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
