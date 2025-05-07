using System;
using LD57.Gameplay;
using LD57.Rendering;

namespace LD57.Editor;

public class WorldEditorBrushTool : BrushTool
{
    private readonly Func<EntityTemplate?> _getTemplate;
    private readonly WorldEditorSurface _surface;

    public WorldEditorBrushTool(EditorSession editorSession, WorldEditorSurface surface, Func<EntityTemplate?> getTemplate) : base(editorSession)
    {
        _getTemplate = getTemplate;
        _surface = surface;
    }
    
    public override TileState GetTileStateInWorldOnHover(TileState original)
    {
        if (EditorSession.IsDraggingSecondary)
        {
            return TileState.TransparentEmpty;
        }

        return _getTemplate()?.CreateAppearance().TileState ?? TileState.TransparentEmpty;
    }

    protected override void OnErase()
    {
        if (!EditorSession.HoveredWorldPosition.HasValue)
        {
            return;
        }
        
        _surface.Data.RemoveEntitiesAtExceptMetadata(EditorSession.HoveredWorldPosition.Value);
    }

    protected override void OnPaint()
    {
        if (!EditorSession.HoveredWorldPosition.HasValue)
        {
            return;
        }

        var template = _getTemplate();

        if (template == null)
        {
            return;
        }
        
        _surface.Data.SetTile(EditorSession.HoveredWorldPosition.Value, template);
    }
}
