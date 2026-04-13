namespace SchoolApplication.Contracts.Classes;

public sealed record ClassResponse(int ClassId, string Name, DateTime? CreatedAtUtc);
