using Sprout.Api.Models;
using Sprout.Api.Services;
using Xunit;

namespace Sprout.Api.Tests;

public class TaskValidatorTests
{
    private static HabitTask Valid() => new() { Label = "Brush Teeth", Emoji = "🪥", SortOrder = 0 };

    [Fact]
    public void Validate_ValidTask_ReturnsNull()
    {
        Assert.Null(TaskValidator.Validate(Valid()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_BlankLabel_ReportsLabelError(string label)
    {
        var errors = TaskValidator.Validate(Valid() with { Label = label });

        Assert.NotNull(errors);
        Assert.Contains("label", errors.Keys);
    }

    [Fact]
    public void Validate_LabelAtMaxLength_IsAccepted()
    {
        Assert.Null(TaskValidator.Validate(Valid() with { Label = new string('a', 100) }));
    }

    [Fact]
    public void Validate_LabelOverMaxLength_ReportsLabelError()
    {
        var errors = TaskValidator.Validate(Valid() with { Label = new string('a', 101) });

        Assert.NotNull(errors);
        Assert.Contains("100", errors["label"][0]);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_BlankEmoji_ReportsEmojiError(string emoji)
    {
        var errors = TaskValidator.Validate(Valid() with { Emoji = emoji });

        Assert.NotNull(errors);
        Assert.Contains("emoji", errors.Keys);
    }

    [Fact]
    public void Validate_EmojiAtMaxLength_IsAccepted()
    {
        Assert.Null(TaskValidator.Validate(Valid() with { Emoji = new string('x', 10) }));
    }

    [Fact]
    public void Validate_EmojiOverMaxLength_ReportsEmojiError()
    {
        var errors = TaskValidator.Validate(Valid() with { Emoji = new string('x', 11) });

        Assert.NotNull(errors);
        Assert.Contains("emoji", errors.Keys);
    }

    [Fact]
    public void Validate_NegativeSortOrder_ReportsSortOrderError()
    {
        var errors = TaskValidator.Validate(Valid() with { SortOrder = -1 });

        Assert.NotNull(errors);
        Assert.Contains("sortOrder", errors.Keys);
    }

    [Fact]
    public void Validate_MultipleInvalidFields_ReportsAllOfThem()
    {
        var errors = TaskValidator.Validate(new HabitTask { Label = "", Emoji = "", SortOrder = -5 });

        Assert.NotNull(errors);
        Assert.Equal(3, errors.Count);
    }
}
