namespace QuakeNote.Shared.Contracts;

public static class Roles
{
    public const string User = "user";
    public const string Admin = "admin";
}

public static class ValidationStatuses
{
    public const string Pending = "pending";
    public const string Verified = "verified";
    public const string Outlier = "outlier";
    public const string Unverified = "unverified";
}

public static class EventVerificationStatuses
{
    public const string Confirmed = "confirmed";
    public const string Disputed = "disputed";
    public const string InsufficientData = "insufficient_data";
}