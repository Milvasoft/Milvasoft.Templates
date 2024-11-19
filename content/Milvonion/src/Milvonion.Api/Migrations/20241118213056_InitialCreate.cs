using Microsoft.EntityFrameworkCore.Migrations;
using Milvonion.Domain;

#nullable disable

namespace Milvonion.Api.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.AddColumn<bool>(name: nameof(MigrationHistory.MigrationCompleted),
                                       schema: "public",
                                       table: TableNames.MigrationHistory,
                                       defaultValue: false);
    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder) { }
}
