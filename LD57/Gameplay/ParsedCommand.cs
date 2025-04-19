using System.Collections.Generic;
using System.Linq;
using ExplogineCore.Data;

namespace LD57.Gameplay;

public class ParsedCommand
{
    private readonly List<string> _remainingArgs;

    public ParsedCommand(string? rawCommand)
    {
        var splitCommand = rawCommand?.Trim().Split() ?? [];

        CommandName = splitCommand.Length == 0 ? string.Empty : splitCommand.First();

        _remainingArgs = splitCommand.ToList();
        _remainingArgs.RemoveAt(0);

        ArgCount = _remainingArgs.Count;
    }

    public string CommandName { get; }
    public int ArgCount { get; }

    public string ArgAsString(int index)
    {
        if (_remainingArgs.IsValidIndex(index))
        {
            return _remainingArgs[index];
        }

        return string.Empty;
    }

    public float ArgAsFloat(int index, float fallback)
    {
        if (float.TryParse(ArgAsString(index), out var result))
        {
            return result;
        }

        return fallback;
    }
}
