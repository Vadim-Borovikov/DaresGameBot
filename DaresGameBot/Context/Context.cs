using AbstractBot;
using DaresGameBot.Context.Meta;
using JetBrains.Annotations;

namespace DaresGameBot.Context;

[UsedImplicitly]
public sealed class Context : IContext<Context, object, MetaContext>
{
    internal Context() { }

    public object? Save() => null;

    public static Context? Load(object data, MetaContext? meta) => null;
}