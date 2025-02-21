// ReSharper disable NullableWarningSuppressionIsUsed

using AbstractBot;
using JetBrains.Annotations;

namespace DaresGameBot.Save;

public sealed class Data : SaveData<object>
{
    [UsedImplicitly]
    public GameData? GameData { get; set; }
}