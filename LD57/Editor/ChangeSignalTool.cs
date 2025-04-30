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

    public TileState GetTileStateInWorldOnHover(TileState original)
    {
        // todo: this should show the color of the signal we'd change to
        return original with {BackgroundColor = Color.LightBlue, BackgroundIntensity = 0.75f};
    }
    
    private void IncrementSignalAt(GridPosition position, int delta)
    {
        foreach (var entity in _editorSession.WorldTemplate.AllEntitiesAt(position))
        {
            var templateName = entity.TemplateName;
            if (string.IsNullOrEmpty(templateName))
            {
                return;
            }

            var template = ResourceAlias.EntityTemplate(templateName);
            if (template.Tags.Contains("Signal"))
            {
                if (entity.ExtraState.TryGetValue("channel", out var result))
                {
                    var newValue = int.Parse(result) + delta;
                    SetChannelValue(entity, newValue);
                }
                else
                {
                    SetChannelValue(entity, 0 + delta);
                }
            }
        }
    }

    private static void SetChannelValue(PlacedEntity entity, int newValue)
    {
        if (!LdResourceAssets.Instance.HasNamedColor($"signal_{newValue}"))
        {
            return;
        }

        entity.ExtraState["channel"] = newValue.ToString();
    }

}
