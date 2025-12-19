namespace Lab1.Task1;

public static class ZipExtension
{
    public static IEnumerable<T[]> CustomZip<T>(
        this IEnumerable<T> first,
        params IEnumerable<T>[] others)
    {
        ArgumentNullException.ThrowIfNull(first);

        ArgumentNullException.ThrowIfNull(others);

        foreach (IEnumerable<T> other in others)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(others));
            }
        }

        IEnumerator<T> e1 = first.GetEnumerator();
        IEnumerator<T>[] otherEnumeratos = others.Select(e => e.GetEnumerator()).ToArray();

        try
        {
            while (e1.MoveNext() && otherEnumeratos.All(e => e.MoveNext()))
            {
                int length = 1 + others.Length;

                var result = new T[length];
                result[0] = e1.Current;

                int i = 1;
                foreach (IEnumerator<T> e in otherEnumeratos)
                {
                    result[i++] = e.Current;
                }

                yield return result;
            }
        }
        finally
        {
            foreach (IEnumerator<T> e in otherEnumeratos)
            {
                e.Dispose();
            }
        }
    }
}