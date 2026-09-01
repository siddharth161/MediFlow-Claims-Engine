using FluentAssertions;
using MediFlow.Application.Claims.Commands;
using MediFlow.Application.Claims.Handlers;
using MediFlow.Domain.Entities;
using MediFlow.Domain.Enums;
using MediFlow.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediFlow.UnitTests.Application;

public class HandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IClaimRepository> _claimRepoMock = new();
    private readonly Mock<IProviderRepository> _providerRepoMock = new();

    public HandlerTests()
    {
        _uowMock.Setup(u => u.Claims).Returns(_claimRepoMock.Object);
        _uowMock.Setup(u => u.Providers).Returns(_providerRepoMock.Object);
    }

    [Fact]
    public async Task SubmitClaimHandler_WhenProviderActive_ShouldPersistClaimAndReturnDto()
    {
        // Arrange
        var provider = Provider.Create("1234567890", "Gregory", "House", "Internal Medicine", "207R00000X", "NJ");
        _providerRepoMock.Setup(r => r.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _claimRepoMock.Setup(r => r.AddAsync(It.IsAny<Claim>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _claimRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                var c = Claim.Submit("PAT-101", "John Doe", provider.Id, ClaimType.Professional, "I10", "Hypertension", DateTime.UtcNow);
                c.AddLineItem("99213", "Visit", ServiceType.Consultation, 150m);
                return c;
            });

        var logger = new Mock<ILogger<SubmitClaimHandler>>();
        var handler = new SubmitClaimHandler(_uowMock.Object, logger.Object);

        var command = new SubmitClaimCommand(
            "PAT-101", "John Doe", provider.Id, ClaimType.Professional,
            "I10", "Hypertension", DateTime.UtcNow,
            [new AddLineItemRequest("99213", "Visit", ServiceType.Consultation, 150m, 1)]);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PatientId.Should().Be("PAT-101");
        _claimRepoMock.Verify(r => r.AddAsync(It.IsAny<Claim>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitClaimHandler_WhenProviderNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _providerRepoMock.Setup(r => r.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var logger = new Mock<ILogger<SubmitClaimHandler>>();
        var handler = new SubmitClaimHandler(_uowMock.Object, logger.Object);

        var command = new SubmitClaimCommand(
            "PAT-101", "John Doe", providerId, ClaimType.Professional,
            "I10", "Hypertension", DateTime.UtcNow, []);

        // Act
        var act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task AdjudicateClaimHandler_WhenApprove_ShouldSetApprovedStatus()
    {
        // Arrange
        var claim = Claim.Submit("PAT-101", "John Doe", Guid.NewGuid(), ClaimType.Professional, "I10", "Hypertension", DateTime.UtcNow);
        _claimRepoMock.Setup(r => r.GetByIdAsync(claim.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(claim);

        var logger = new Mock<ILogger<AdjudicateClaimHandler>>();
        var handler = new AdjudicateClaimHandler(_uowMock.Object, logger.Object);

        var command = new AdjudicateClaimCommand(claim.Id, Approve: true, ApprovedAmount: 120m, DenialReason: null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ClaimStatus.Approved.ToString());
        result.ApprovedAmount.Should().Be(120m);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
