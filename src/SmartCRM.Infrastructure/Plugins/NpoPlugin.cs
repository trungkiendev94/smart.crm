using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using SmartCRM.Domain.Entities;
using SmartCRM.Infrastructure.Persistence;

namespace SmartCRM.Infrastructure.Plugins;

public class NpoPlugin
{
    private readonly SmartCrmDbContext _context;

    public NpoPlugin(SmartCrmDbContext context)
    {
        _context = context;
    }

    [KernelFunction, Description("Get list of active fundraising campaigns.")]
    public async Task<string> GetActiveCampaignsAsync()
    {
        var campaigns = await _context.Campaigns
            .Where(c => c.Status == "Active")
            .Select(c => new { c.Name, c.Description, c.CurrentAmount, c.TargetAmount })
            .ToListAsync();

        if (!campaigns.Any()) return "There are currently no active fundraising campaigns.";

        return "List of campaigns:\n" + string.Join("\n", campaigns.Select(c => $"- {c.Name}: {c.Description} (Raised: {c.CurrentAmount}/{c.TargetAmount})"));
    }

    [KernelFunction, Description("Register a new donor.")]
    public async Task<string> RegisterDonorAsync(
        [Description("Full name of the donor")] string fullName,
        [Description("Donor email")] string email,
        [Description("Phone number")] string? phone = null,
        [Description("Donor type (Individual or Corporate)")] string donorType = "Individual")
    {
        var donor = new Donor
        {
            FullName = fullName,
            Email = email,
            Phone = phone,
            DonorType = donorType,
            TotalDonatedAmount = 0
        };

        _context.Donors.Add(donor);
        await _context.SaveChangesAsync();

        return $"Registered donor {fullName} successfully.";
    }

    [KernelFunction, Description("Record a donation from a donor to a campaign.")]
    public async Task<string> RecordDonationAsync(
        [Description("Email of the donor")] string donorEmail,
        [Description("Name of the fundraising campaign")] string campaignName,
        [Description("Donation amount")] decimal amount,
        [Description("Payment method")] string paymentMethod)
    {
        var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Email == donorEmail);
        if (donor == null) return "No donor found with this email.";

        var campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.Name.Contains(campaignName));
        if (campaign == null) return "No fundraising campaign found with this name.";

        var donation = new Donation
        {
            DonorId = donor.Id,
            CampaignId = campaign.Id,
            Amount = amount,
            DonationDate = DateTime.UtcNow,
            PaymentMethod = paymentMethod,
            Status = "Completed"
        };

        _context.Donations.Add(donation);
        
        donor.TotalDonatedAmount += amount;
        campaign.CurrentAmount += amount;

        await _context.SaveChangesAsync();

        return $"Recorded donation of {amount} from {donor.FullName} for campaign {campaign.Name}.";
    }
}
