using System.Diagnostics;
using System.Runtime.CompilerServices;
using Grpc.Core;

namespace gRPCFullDuplex.Shared.Extensions;

public static class AsyncStreamReaderExtension
{
    public static async IAsyncEnumerable<int> ReadAllPrimeAsync(this IAsyncStreamReader<SieveRequest> streamReader, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await streamReader.MoveNext(cancellationToken).ConfigureAwait(false))
        {
            yield return streamReader.Current.Prime;
        }
    }
}