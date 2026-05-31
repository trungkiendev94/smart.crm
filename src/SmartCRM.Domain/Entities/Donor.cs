namespace SmartCRM.Domain.Entities;

public class Donor : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string DonorType { get; set; } = "Individual"; // Individual, Corporate
    public decimal TotalDonatedAmount { get; set; }
}