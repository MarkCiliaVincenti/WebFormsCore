namespace WebFormsCore.UI.Attributes;

public class NullableAttributeParser<T> : IAttributeParser<T?>
    where T : struct
{
    private readonly IAttributeParser<T> _parser;

    public NullableAttributeParser(IAttributeParser<T> parser)
    {
        _parser = parser;
    }

    public T? Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return _parser.Parse(value);
    }
}
