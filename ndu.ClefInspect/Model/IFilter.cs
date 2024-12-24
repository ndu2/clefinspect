namespace ndu.ClefInspect.Model
{
    public interface IFilter
    {
        IMatcher Create();

        bool AcceptsAll { get; }
        bool AcceptsNone { get; }
    }
}
