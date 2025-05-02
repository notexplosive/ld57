namespace LD57.Editor;

public interface IEditorSelector
{
}

public class EditorSelector<T> : IEditorSelector where T : class
{
    public T? Selected { get; set; }

    public bool IsSelected(T? selectableContent)
    {
        if (selectableContent == null || Selected == null)
        {
            return false;
        }
        
        return selectableContent == Selected;
    }
}
