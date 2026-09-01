using MediFlow.Domain.Entities;
using MediFlow.Domain.Enums;
using MediFlow.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;

namespace MediFlow.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(MediFlowDbContext context, ILogger logger)
    {
        if (context.Providers.Any())
        {
            return; // DB already seeded
        }

        logger.LogInformation("Seeding database with healthcare providers and sample Medicaid/Medicare claims...");

        var provider1 = Provider.Create(
            "1234567890", "Sarah", "Connor", "Cardiology", "207RC0000X", "IL", ProviderNetworkStatus.InNetwork);

        var provider2 = Provider.Create(
            "9876543210", "Gregory", "House", "Internal Medicine", "207R00000X", "NJ", ProviderNetworkStatus.InNetwork);

        var provider3 = Provider.Create(
            "1122334455", "Meredith", "Grey", "General Surgery", "208600000X", "WA", ProviderNetworkStatus.InNetwork);

        context.Providers.AddRange(provider1, provider2, provider3);
        await context.SaveChangesAsync();

        // Sample Claim 1: Cardiology consultation + ECG (Approved)
        var claim1 = Claim.Submit(
            "PAT-10023", "Johnathan Doe",
            provider1.Id, ClaimType.Professional,
            "I25.10", "Atherosclerotic heart disease of native coronary artery",
            DateTime.UtcNow.AddDays(-5));

        claim1.AddLineItem("99214", "Level 4 Office Visit (Established)", ServiceType.Consultation, 185.00m);
        claim1.AddLineItem("93000", "Electrocardiogram (ECG) 12-lead", ServiceType.Diagnostic, 75.00m);
        claim1.Approve(240.00m);

        // Sample Claim 2: Emergency Surgery (Pending Review)
        var claim2 = Claim.Submit(
            "PAT-10088", "Eleanor Vance",
            provider3.Id, ClaimType.Institutional,
            "K35.80", "Unspecified acute appendicitis",
            DateTime.UtcNow.AddDays(-2));

        claim2.AddLineItem("44970", "Laparoscopic Appendectomy", ServiceType.Surgery, 3200.00m);
        claim2.AddLineItem("99285", "Emergency Dept Visit (High Severity)", ServiceType.Emergency, 850.00m);
        claim2.MarkUnderReview();

        // Sample Claim 3: Out-of-network preventative screening (Denied)
        var claim3 = Claim.Submit(
            "PAT-10145", "Robert Paulson",
            provider2.Id, ClaimType.Professional,
            "Z00.00", "Encounter for general adult medical exam",
            DateTime.UtcNow.AddDays(-1));

        claim3.AddLineItem("99396", "Preventative Visit Adult 40-64 yrs", ServiceType.Preventive, 220.00m);
        claim3.Deny("Service exceeded annual benefit frequency limit for preventative encounters.");

        context.Claims.AddRange(claim1, claim2, claim3);
        await context.SaveChangesAsync();

        logger.LogInformation("Database successfully seeded.");
    }
}
