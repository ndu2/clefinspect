namespace clef_inspect.Model
{
    public interface IMatcher
    {
        bool Accept(ClefLine line);
    }
}
