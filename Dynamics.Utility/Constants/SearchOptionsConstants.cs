namespace Dynamics.Utility;

public static class SearchOptionsConstants
{
    public const string Name  = "Name";
    public const string Money  = "Money";
    public const string Resource  = "Resource";
    public const string Title  = "Title";
    public const string Content  = "Content";
    public const string Filter  = "Filter";
    public const string StatusPending = "Pending";
    public const string StatusAccepted = "Accepted";
    public const string StatusDenied = "Denied";
}

public enum TransactionSearchOptions
{
    Money,
    Resource,
    Title,
    Content,
    Filter, // This should be the default value for all filters
    Pending,
    Denied,
    Accepted,
    Organization,
    Project,
    User,
}