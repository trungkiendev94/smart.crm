namespace SmartCRM.Domain.Entities;

public class Donation : BaseEntity
{
    public Guid DonorId { get; set; }
    public Guid CampaignId { get; set; }
    public decimal Amount { get; set; }
    public DateTime DonationDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed"; // Pending, Completed, Failed
}