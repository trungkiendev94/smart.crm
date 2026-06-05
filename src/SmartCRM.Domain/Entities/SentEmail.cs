using System;

namespace SmartCRM.Domain.Entities;

public class SentEmail : BaseEntity
{
    public string RecipientEmail { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
