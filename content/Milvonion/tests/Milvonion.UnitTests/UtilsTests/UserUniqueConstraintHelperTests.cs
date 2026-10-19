using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.Helpers;
using Npgsql;

namespace Milvonion.UnitTests.UtilsTests;

/// <summary>
/// <see cref="UserUniqueConstraintHelper"/>: unique-index violations on Users map to the pre-check message keys.
/// </summary>
[Trait("Utils Unit Tests", "User unique constraint helper.")]
public class UserUniqueConstraintHelperTests
{
    private static DbUpdateException Wrap(string sqlState, string constraintName) => new("update failed", new PostgresException("duplicate key", "ERROR", "ERROR", sqlState,
        detail: null, hint: null, position: 0, internalPosition: 0, internalQuery: null, where: null,
        schemaName: "public", tableName: "Users", columnName: null, dataTypeName: null, constraintName: constraintName, file: null, line: null, routine: null));

    [Theory]
    [InlineData("IX_Users_NormalizedUserName", MessageKey.UserExists)]
    [InlineData("IX_Users_UserName_IsDeleted_DeletionDate", MessageKey.UserExists)]
    [InlineData("IX_Users_NormalizedEmail", MessageKey.EmailExists)]
    public void GetDuplicateMessageKey_ShouldMapKnownUserIndexes(string constraint, string expectedKey)
    {
        UserUniqueConstraintHelper.GetDuplicateMessageKey(Wrap("23505", constraint)).Should().Be(expectedKey);
    }

    [Theory]
    [InlineData("PK_Users")]
    [InlineData("ix_users_normalizedusername")]
    [InlineData("")]
    [InlineData(null)]
    public void GetDuplicateMessageKey_ShouldReturnNull_ForOtherConstraints(string constraint)
    {
        UserUniqueConstraintHelper.GetDuplicateMessageKey(Wrap("23505", constraint)).Should().BeNull();
    }

    [Theory]
    [InlineData("23503")]
    [InlineData("40001")]
    public void GetDuplicateMessageKey_ShouldReturnNull_WhenNotAUniqueViolation(string sqlState)
    {
        UserUniqueConstraintHelper.GetDuplicateMessageKey(Wrap(sqlState, "IX_Users_NormalizedUserName")).Should().BeNull();
    }

    [Fact]
    public void GetDuplicateMessageKey_ShouldReturnNull_WhenInnerExceptionIsNotPostgres()
    {
        var exception = new DbUpdateException("update failed", new InvalidOperationException("IX_Users_NormalizedUserName"));

        UserUniqueConstraintHelper.GetDuplicateMessageKey(exception).Should().BeNull();
    }

    [Fact]
    public void GetDuplicateMessageKey_ShouldReturnNull_WhenNoInnerExceptionOrNullArgument()
    {
        UserUniqueConstraintHelper.GetDuplicateMessageKey(new DbUpdateException("plain")).Should().BeNull();
        UserUniqueConstraintHelper.GetDuplicateMessageKey(null).Should().BeNull();
    }
}
