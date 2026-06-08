using Sprout.Api.Models;

namespace Sprout.Api.Services;

public static class TaskValidator
{
    private const int MaxLabelLength = 100;
    private const int MaxEmojiLength = 10;

    public static Dictionary<string, string[]>? Validate(HabitTask task)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(task.Label))
        {
            errors["label"] = ["Label is required."];
        }
        else if (task.Label.Length > MaxLabelLength)
        {
            errors["label"] = [$"Label must not exceed {MaxLabelLength} characters."];
        }

        if (string.IsNullOrWhiteSpace(task.Emoji))
        {
            errors["emoji"] = ["Emoji is required."];
        }
        else if (task.Emoji.Length > MaxEmojiLength)
        {
            errors["emoji"] = ["Emoji is too long."];
        }

        if (task.SortOrder < 0)
        {
            errors["sortOrder"] = ["Sort order must be non-negative."];
        }

        return errors.Count > 0 ? errors : null;
    }
}
