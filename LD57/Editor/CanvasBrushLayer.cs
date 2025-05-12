namespace LD57.Editor;

public class CanvasBrushLayer
{
    public CanvasBrushLayer(bool isVisible, bool isEditing)
    {
        IsVisible = isVisible;
        IsEditing = isEditing;
    }

    public bool IsVisible { get; set; }

    /// <summary>
    /// Reflects if we're allowed to Edit right now
    /// </summary>
    public bool IsFunctionallyActive => IsVisible && IsEditing;

    /// <summary>
    /// Reflects if we've explicitly permitted editing or not
    /// </summary>
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
