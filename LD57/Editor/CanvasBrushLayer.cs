namespace LD57.Editor;

public class CanvasBrushLayer
{
    public CanvasBrushLayer(bool isVisible, bool isEditing)
    {
        IsVisible = isVisible;
        IsEditing = isEditing;
    }

    public bool IsVisible { get; set; }
    public bool IsEditing { get; set; }

    public void ToggleVisible()
    {
        IsVisible = !IsVisible;
    }

    public void ToggleEditing()
    {
        IsEditing = !IsEditing;
    }
}
