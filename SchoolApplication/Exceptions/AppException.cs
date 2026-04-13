namespace SchoolApplication.Exceptions;

/// <summary>Base type for business-rule failures mapped to 4xx responses.</summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}
