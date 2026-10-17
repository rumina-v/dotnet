namespace UserRegistration.Api.Logging;

public static class LogEvents
{
    public static readonly EventId UserCreated = new(1001, nameof(UserCreated));
    public static readonly EventId LoginSucceeded = new(1002, nameof(LoginSucceeded));
    public static readonly EventId UserUpdated = new(1003, nameof(UserUpdated));
    public static readonly EventId UserDeleted = new(1004, nameof(UserDeleted));
    public static readonly EventId UsersListed = new(1005, nameof(UsersListed));
    public static readonly EventId DatabaseReady = new(1006, nameof(DatabaseReady));

    public static readonly EventId DuplicateLogin = new(2001, nameof(DuplicateLogin));
    public static readonly EventId LoginFailed = new(2002, nameof(LoginFailed));
    public static readonly EventId UserNotFound = new(2003, nameof(UserNotFound));
    public static readonly EventId ValidationFailed = new(2004, nameof(ValidationFailed));

    public static readonly EventId RequestFailed = new(3001, nameof(RequestFailed));
    public static readonly EventId StartupFailed = new(3002, nameof(StartupFailed));
}
