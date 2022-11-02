namespace gRPCFullDuplex.Shared.Contract;

public interface ISieveResponse
{
    int Index { get; }
    int Value { get; }
}
public interface IPrimeResponse : ISieveResponse { }
public interface INotPrimeResponse : ISieveResponse { }

public record SieveResponse(int Index, int Value) : ISieveResponse;
public record PrimeResponse(int Index, int Value) : SieveResponse(Index, Value), IPrimeResponse { }
public record NotPrimeResponse(int Index, int Value) : SieveResponse(Index, Value), INotPrimeResponse { }