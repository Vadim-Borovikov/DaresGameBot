using System;

namespace DaresGameBot;

internal interface IGuidProvider
{
    public Guid? Guid { get; }
}