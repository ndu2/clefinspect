namespace clef_inspect.Model
{
    public interface IFilter
    {
        IMatcher Create();

        bool AcceptsAll { get; }
        bool AcceptsNone { get; }
    }
}
