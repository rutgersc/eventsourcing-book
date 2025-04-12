namespace Deciders;

public static class ResultExtensions
{
    public static (IReadOnlyCollection<TOk>, IReadOnlyCollection<TErr>) CollectResults<TOk, TErr>(this IEnumerable<Result<TOk, TErr>> results)
        where TOk : notnull
        where TErr : notnull
    {
        var oks = new List<TOk>();
        var errs = new List<TErr>();

        foreach (var result in results)
        {
            switch (result)
            {
                case Result<TOk, TErr>.Ok(var ok):
                    oks.Add(ok);
                    break;

                case Result<TOk, TErr>.Error(var err):
                    errs.Add(err);
                    break;

            }
        }

        return (oks, errs);
    }
}

