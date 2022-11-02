namespace gRPCFullDuplex.Shared.Contract;

public interface IEratosthenesSieveAlgorithm
{
    IAsyncEnumerable<ISieveResponse> SearchAsync(int start, int range, IAsyncEnumerable<int> primesAsyncEnumerable, CancellationToken token);
    IEnumerable<ISieveResponse> Search(int start, int range, CancellationToken token);
}