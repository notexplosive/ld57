using System;
using System.Collections.Generic;
using LD57.Gameplay;
using LD57.Rendering;

namespace LD57.Editor;

public class WorldSelectionTool : SelectionTool
{
    private readonly Func<EntityTemplate?> _getTemplate;
    private readonly WorldEditorSurface _surface;

    public WorldSelectionTool(EditorSession editorSession, WorldEditorSurface surface,
        Func<EntityTemplate?> getTemplate) : base(editorSession)
    {
        _surface = surface;
        _getTemplate = getTemplate;
    }

    protected override IEditorSurface Surface => _surface;

    protected override void FillWithCurrentTemplate(List<GridPosition> positions)
    {
        var template = _getTemplate();

        if (template != null)
        {
            _surface.Data.FillAllPositions(positions, template);
        }
    }
}
