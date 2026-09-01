namespace MediFlow.Domain.Enums;

/// <summary>
/// Represents the current status of a healthcare claim in its lifecycle.
/// </summary>
public enum ClaimStatus
{
    Submitted = 0,
    UnderReview = 1,
    Approved = 2,
    Denied = 3,
    Appealed = 4,
    Paid = 5
}

/// <summary>
/// Type of healthcare claim being submitted.
/// </summary>
public enum ClaimType
{
    Professional = 0,   // CMS-1500 / EDI 837P
    Institutional = 1,  // UB-04 / EDI 837I
    Dental = 2          // ADA / EDI 837D
}

/// <summary>
/// Type of healthcare service line on a claim.
/// </summary>
public enum ServiceType
{
    Consultation = 0,
    Diagnostic = 1,
    Surgery = 2,
    Preventive = 3,
    Emergency = 4,
    Pharmacy = 5,
    Rehabilitation = 6
}

/// <summary>
/// Network status of a healthcare provider.
/// </summary>
public enum ProviderNetworkStatus
{
    InNetwork = 0,
    OutOfNetwork = 1,
    Terminated = 2
}
