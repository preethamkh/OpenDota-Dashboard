using DotaDashboard.Data;
using DotaDashboard.Models.Entities;
using DotaDashboard.Services.Implementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotaDashboard.Tests.Services;

public class AggregateStatsServiceTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<AggregateStatsService>> _logger;
    private readonly AggregateStatsService _service;

    public AggregateStatsServiceTests()
    {
        _context = GetInMemoryDbContext();
        _logger = new Mock<ILogger<AggregateStatsService>>();
        _service = new AggregateStatsService(_context, _logger.Object);
    }

    private ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetTopHeroesByWinRate_ReturnsCorrectOrder()
    {
        // Arrange
        _context.Heroes.AddRange(
            new Hero { HeroId = 1, Name = "Hero1", TotalPicks = 20, TotalWins = 18, LastUpdated = DateTime.UtcNow }, // 90% WR
            new Hero { HeroId = 2, Name = "Hero2", TotalPicks = 20, TotalWins = 10, LastUpdated = DateTime.UtcNow }, // 50% WR
            new Hero { HeroId = 3, Name = "Hero3", TotalPicks = 20, TotalWins = 16, LastUpdated = DateTime.UtcNow }  // 80% WR
        );

        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTopHeroesByWinRateAsync(3, 10);

        // Assert
        result.Count.Should().Be(3);
        result[0].Name.Should().Be("Hero1");
        result[0].WinRate.Should().Be(90);
        result[1].Name.Should().Be("Hero3");
        result[2].Name.Should().Be("Hero2");
    }
}
