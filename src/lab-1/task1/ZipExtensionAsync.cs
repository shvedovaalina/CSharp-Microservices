namespace Lab1.Task1;

public static class ZipExtensionAsync
{
    public static async IAsyncEnumerable<T[]> CustomZipAsync<T>(
        this IAsyncEnumerable<T> first,
        params IAsyncEnumerable<T>[] others)
    {
        ArgumentNullException.ThrowIfNull(first);

        ArgumentNullException.ThrowIfNull(others);

        foreach (IAsyncEnumerable<T> other in others)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(others));
            }
        }

        IAsyncEnumerator<T> e1 = first.GetAsyncEnumerator();
        IAsyncEnumerator<T>[] otherEnumeratos = others.Select(e => e.GetAsyncEnumerator()).ToArray();

        try
        {
            while (await e1.MoveNextAsync())
            {
                int length = 1 + others.Length;

                var result = new T[length];
                result[0] = e1.Current;

                int i = 1;
                bool flag = true;
                foreach (IAsyncEnumerator<T> e in otherEnumeratos)
                {
                    if (!await e.MoveNextAsync())
                    {
                        flag = false;
                    }

                    result[i++] = e.Current;
                }

                if (!flag)
                {
                    yield break;
                }

                yield return result;
            }
        }
        finally
        {
            foreach (IAsyncEnumerator<T> e in otherEnumeratos)
            {
                await e.DisposeAsync();
            }
        }
    }
}