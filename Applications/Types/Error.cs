namespace App.Applications;

public record Error
{
    public static readonly Error None = new (0, string.Empty, string.Empty);

    public static readonly Error NullValue = new (
        0, "General.Null", "Null value was provided.");

    public Error(int statusCode, string title, string description)
    {
        StatusCode = statusCode;
        Title = title;
        Description = description;
    }

    public int StatusCode { get; }

    public string Title { get; }

    public string Description { get; }

    public static Error BadRequest(string title, string description) =>
        new (400, title, description);

    public static Error Conflict(string title, string description) =>
        new (409, title, description);

    public static Error Forbidden(string title, string description) =>
        new (403, title, description);

    public static Error NotFound(string title, string description) =>
        new (404, title, description);
}
