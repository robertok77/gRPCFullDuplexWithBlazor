namespace gRPCFullDuplex.Blazor.Application.Handlers.Query;

public record GenerateSetting(int Range)
{
    public readonly int Start = 2;
    public int Range { get; } = Range;
    public int WebRange => Range / 2;
    public int ServerStart => Start + WebRange;
    public int ServerRange => Range / 2;
}