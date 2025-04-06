using System.Collections.Generic;
using ExplogineCore;
using ExplogineCore.Data;

namespace LD57.Gameplay;

public class MessageContent
{
    private readonly string _rawContent;
    private readonly List<MessagePage> _pages = new();

    public MessageContent(string rawContent)
    {
        _rawContent = rawContent;

        var lines = rawContent.SplitLines();

        var currentPage = new MessagePage();

        foreach (var line in lines)
        {
            if (line == "#")
            {
                _pages.Add(currentPage);
                currentPage = new MessagePage();
                continue;
            }

            currentPage.AddLine(line);
        }

        if (currentPage.HasContent())
        {
            _pages.Add(currentPage);
        }
    }

    public MessagePage? GetPage(int pageIndex)
    {
        if (_pages.IsValidIndex(pageIndex))
        {
            return _pages[pageIndex];
        }

        return null;
    }
}
