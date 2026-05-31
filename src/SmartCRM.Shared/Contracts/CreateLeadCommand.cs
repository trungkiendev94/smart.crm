namespace SmartCRM.Shared.Contracts;

public sealed record CreateLeadCommand
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
