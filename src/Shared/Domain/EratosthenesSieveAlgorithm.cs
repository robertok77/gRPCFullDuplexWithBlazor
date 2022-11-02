using System.Runtime.CompilerServices;
using gRPCFullDuplex.Shared.Contract;
using Microsoft.Extensions.Logging;

namespace gRPCFullDuplex.Shared.Domain;

/// <summary>
/// Eratosthenes Sieve implementation.
/// Additional method added to support incoming array of already generated prime numbers
/// </summary>
public class EratosthenesSieveAlgorithm : IEratosthenesSieveAlgorithm
{
    private readonly ILogger<EratosthenesSieveAlgorithm> _logger;
    private int[] _sieve = Array.Empty<int>();

    public EratosthenesSieveAlgorithm(ILogger<EratosthenesSieveAlgorithm> logger) => _logger = logger;

    #region IEratosthenesSieve

    /// <summary>
    /// Searching for primes in sieve beginning from start and length range. First begins with primes IAsyncEnumerable incoming from primesAsyncEnumerable 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="range"></param>
    /// <param name="primesAsyncEnumerable"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<ISieveResponse> SearchAsync(int start, int range, IAsyncEnumerable<int> primesAsyncEnumerable, [EnumeratorCancellation] CancellationToken token)
    {
        _sieve = GenerateSieve(start, range);
        await foreach (var prime in primesAsyncEnumerable.WithCancellation(token).ConfigureAwait(false))
        {
            _logger.LogDebug("Prime {prime} generated:", prime);

            foreach (var nominator in NominatorLoop(prime, 0, token))
                yield return nominator;
        }
        await Task.Yield();
        foreach (var sieveResponse in Search(token))
        {
            yield return sieveResponse;
        }
    }

    /// <summary>
    /// Searching for primes in sieve beginning from start and length range.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="range"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public IEnumerable<ISieveResponse> Search(int start, int range, CancellationToken token)
    {
        _sieve = GenerateSieve(start, range);
        return Search(token);
    }
    #endregion
    #region Private
    private int[] GenerateSieve(int start, int range) => Enumerable.Range(start, range).ToArray();
    private IEnumerable<ISieveResponse> Search(CancellationToken token)
    {
        for (var denominatorIdx = 0; denominatorIdx < _sieve.Length && !token.IsCancellationRequested; denominatorIdx++)
        {
            var denominator = _sieve[denominatorIdx];
            if (denominator == 0) continue;

            _logger.LogDebug("Prime generated. index:{index}; value:{value}", denominatorIdx, denominator); 

            yield return new PrimeResponse(denominatorIdx, denominator);

            foreach (var nominator in NominatorLoop(denominator, denominatorIdx, token))
            {
                yield return nominator;
            }
        }
    }
    private IEnumerable<ISieveResponse> NominatorLoop(int denominator, int denominatorIdx, CancellationToken token)
    {
        for (var nominatorIdx = denominatorIdx; nominatorIdx < _sieve.Length && !token.IsCancellationRequested; nominatorIdx++)
        {
            var nominator = _sieve[nominatorIdx];
            if (nominator == 0) continue;
            if (denominator != nominator && nominator % denominator == 0)
            {
                _sieve[nominatorIdx] = 0;
                yield return new NotPrimeResponse(nominatorIdx, nominator);
            }
        }
    }
    #endregion
}