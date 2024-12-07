namespace clef_inspect.Model
{
    public interface IFilter
    {
        IMatcher Create();

        bool AccceptsAll { get; }
    }
}
