using GoogleSheetsManager.Extensions;

namespace DaresGameBot.Operations.Infos;

internal sealed class TogglePreferenceInfo
{
    public readonly long PartnerId;

    private TogglePreferenceInfo(long partnerId) => PartnerId = partnerId;

    public static TogglePreferenceInfo? From(string value)
    {
        long? partnerId = value.ToLong();
        return partnerId.HasValue ? new TogglePreferenceInfo(partnerId.Value) : null;
    }
}