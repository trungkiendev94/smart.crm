namespace SmartCRM.Domain.Entities;

public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = string.Empty; // e.g., "AgentInstructions"
    public string Value { get; set; } = string.Empty;
}
