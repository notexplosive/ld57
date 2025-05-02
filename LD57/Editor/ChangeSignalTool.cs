using System.Collections.Generic;
using System.Linq;
using ExplogineMonoGame;
using ExplogineMonoGame.Input;
using LD57.CartridgeManagement;
using LD57.Gameplay;
using LD57.Rendering;
using Microsoft.Xna.Framework;

namespace LD57.Editor;

public class ChangeSignalTool : IEditorTool
{
    private readonly EditorSession _editorSession;

    public ChangeSignalTool(EditorSession editorSession)
    {
        _editorSession = editorSession;
    }

    public TileState TileStateInToolbar => TileState.Sprite(ResourceAlias.Tools, 4);

    public string Status()
    {
        if (_editorSession.HoveredWorldPosition.HasValue)
        {
            var entity = GetSignalEntitiesAt(_editorSession.HoveredWorldPosition.Value).FirstOrDefault();

            if (entity != null)
            {
                return "[LMB]+ [RMB]-";
            }
        }

        return "Change Signal Color";
    }

    public void UpdateInput(ConsumableInput.ConsumableKeyboard inputKeyboard)
    {
    }

    public void StartMousePressInWorld(GridPosition position, MouseButton mouseButton)
    {
        if (mouseButton == MouseButton.Left)
        {
            IncrementSignalAt(position, 1);
        }

        if (mouseButton == MouseButton.Right)
        {
            IncrementSignalAt(position, -1);
        }
    }

    public void FinishMousePressInWorld(GridPosition? position, MouseButton mouseButton)
    {
    }

    public void PaintToScreen(AsciiScreen screen, GridPosition cameraPosition)
    {
    }

    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        if (!_editorSession.HoveredWorldPosition.HasValue)
        {
            return original;
        }

        var entity = GetSignalEntitiesAt(_editorSession.HoveredWorldPosition.Value).FirstOrDefault();

        if (entity == null)
        {
            return original;
        }

        var channelValue = GetChannelValue(entity);

        var color = GetColorForSignalIfExists(channelValue);
        if (!color.HasValue)
        {
            return original;
        }

        return original.WithBackground(color.Value) with {ForegroundColor = Color.White};
    }

    private void IncrementSignalAt(GridPosition position, int delta)
    {
        foreach (var entity in GetSignalEntitiesAt(position))
        {
            SetChannelValue(entity, GetChannelValue(entity) + delta);
        }
    }

    private int GetChannelValue(PlacedEntity entity)
    {
        if (entity.ExtraState.TryGetValue("channel", out var result))
        {
            if (int.TryParse(result, out var parseResult))
            {
                return parseResult;
            }
        }

        return 0;
    }

    private IEnumerable<PlacedEntity> GetSignalEntitiesAt(GridPosition position)
    {
        foreach (var entity in _editorSession.WorldTemplate.AllEntitiesAt(position))
        {
            var templateName = entity.TemplateName;
            if (string.IsNullOrEmpty(templateName))
            {
                yield break;
            }

            var template = ResourceAlias.EntityTemplate(templateName) ?? new EntityTemplate();
            if (template.Tags.Contains("Signal"))
            {
                yield return entity;
            }
        }
    }

    private static void SetChannelValue(PlacedEntity entity, int newValue)
    {
        var color = GetColorForSignalIfExists(newValue);
        if (!color.HasValue)
        {
            return;
        }

        entity.ExtraState["channel"] = newValue.ToString();
    }

    private static Color? GetColorForSignalIfExists(int newValue)
    {
        var key = $"signal_{newValue}";
        if (LdResourceAssets.Instance.HasNamedColor(key))
        {
            return LdResourceAssets.Instance.GetNamedColor(key);
        }

        return null;
    }
}
