namespace ndu.ClefInspect.Model
{
    public interface IMatcher
    {
        bool Accept(ClefLine line);
    }
}
