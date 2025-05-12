namespace LD57.Editor;

public class CanvasBrushLayer
{
    private bool _isEditing;

    public CanvasBrushLayer(bool isVisible, bool isEditing)
    {
        IsVisible = isVisible;
        IsEditing = isEditing;
    }

    public bool IsVisible { get; set; }

    public bool IsEditing
    {
        get => IsVisible && _isEditing;
        set => _isEditing = value;
    }

    public void ToggleVisible()
    {
        IsVisible = !IsVisible;
    }

    public void ToggleEditing()
    {
        _isEditing = !_isEditing;
    }
}
