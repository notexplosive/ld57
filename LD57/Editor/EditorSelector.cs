namespace LD57.Editor;

public class EditorSelector
{
    private SelectableButton? _selected;

    public SelectableButton? Selected
    {
        get
        {
            return _selected;
        }
        set
        {
            _selected = value;
            if (value != null)
            {
                value.OnSelect();
            }
        }
    }
}
