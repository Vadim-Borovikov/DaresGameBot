namespace DaresGameBot;

internal static class ObjectExtensions
{
    public static ushort? ToUshort(this object? o)
    {
        if (o is ushort u)
        {
            return u;
        }
        return ushort.TryParse(o?.ToString(), out u) ? u : null;
    }
}