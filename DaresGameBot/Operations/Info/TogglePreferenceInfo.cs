using GoogleSheetsManager.Extensions;

namespace DaresGameBot.Operations.Info;

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