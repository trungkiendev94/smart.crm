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

    [KernelFunction, Description("Lấy danh sách các chiến dịch gây quỹ (Campaigns) đang hoạt động.")]
    public async Task<string> GetActiveCampaignsAsync()
    {
        var campaigns = await _context.Campaigns
            .Where(c => c.Status == "Active")
            .Select(c => new { c.Name, c.Description, c.CurrentAmount, c.TargetAmount })
            .ToListAsync();

        if (!campaigns.Any()) return "Hiện không có chiến dịch gây quỹ nào đang hoạt động.";

        return "Danh sách chiến dịch:\n" + string.Join("\n", campaigns.Select(c => $"- {c.Name}: {c.Description} (Đã huy động: {c.CurrentAmount}/{c.TargetAmount})"));
    }

    [KernelFunction, Description("Đăng ký một nhà tài trợ (Donor) mới.")]
    public async Task<string> RegisterDonorAsync(
        [Description("Tên đầy đủ của nhà tài trợ")] string fullName,
        [Description("Email nhà tài trợ")] string email,
        [Description("Số điện thoại")] string? phone = null,
        [Description("Loại nhà tài trợ (Individual hoặc Corporate)")] string donorType = "Individual")
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

        return $"Đã đăng ký nhà tài trợ {fullName} thành công.";
    }

    [KernelFunction, Description("Ghi nhận một khoản đóng góp (Donation) từ nhà tài trợ cho một chiến dịch.")]
    public async Task<string> RecordDonationAsync(
        [Description("Email của nhà tài trợ")] string donorEmail,
        [Description("Tên chiến dịch gây quỹ")] string campaignName,
        [Description("Số tiền đóng góp")] decimal amount,
        [Description("Phương thức thanh toán")] string paymentMethod)
    {
        var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Email == donorEmail);
        if (donor == null) return "Không tìm thấy nhà tài trợ với email này.";

        var campaign = await _context.Campaigns.FirstOrDefaultAsync(c => c.Name.Contains(campaignName));
        if (campaign == null) return "Không tìm thấy chiến dịch gây quỹ này.";

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

        return $"Đã ghi nhận khoản đóng góp {amount} từ {donor.FullName} cho chiến dịch {campaign.Name}.";
    }
}
