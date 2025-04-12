namespace EventsourcingBook.Infra;

record MockTransaction(Action Commit, Action Rollback)
{
    public static MockTransaction Create() => new(() => { }, () => { });
};
