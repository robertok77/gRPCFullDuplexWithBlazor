namespace gRPCFullDuplex.Shared.Model;

public record SerilogSetting
{
    public Minimumlevel MinimumLevel { get; set; }
}

public record Minimumlevel
{
    public string Default { get; init; } 
    public Override Override { get; init; }
}

public record Override
{
    public string Microsoft { get; init; }
    public string MicrosoftHostingLifetime { get; init; }
}