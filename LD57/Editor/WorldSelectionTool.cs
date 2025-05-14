using System;
using System.Collections.Generic;
using LD57.Gameplay;
using LD57.Rendering;

namespace LD57.Editor;

public class WorldSelectionTool : SelectionTool
{
    private readonly WorldEditorBrushFilter _worldEditorBrushFilter;
    private readonly Func<EntityTemplate?> _getTemplate;

    public WorldSelectionTool(EditorSession editorSession, WorldEditorSurface surface, WorldEditorBrushFilter worldEditorBrushFilter,
        Func<EntityTemplate?> getTemplate) : base(editorSession)
    {
        Surface = surface;
        _worldEditorBrushFilter = worldEditorBrushFilter;
        _getTemplate = getTemplate;
    }

    protected override WorldEditorSurface Surface { get; }

    protected override void FillWithCurrentInk(List<GridPosition> positions)
    {
        var template = _getTemplate();

        if (template != null)
        {
            Surface.Data.FillAllPositions(positions, template, _worldEditorBrushFilter);
        }
    }
}
