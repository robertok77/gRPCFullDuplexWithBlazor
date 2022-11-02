using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gRPCFullDuplex.Tests.Helpers;

internal static class Extensions
{
    public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(this IEnumerable<T> enumerable)
    {
        foreach (var i in enumerable)
            yield return i;
    }
}