using DotaDashboard.Data;
using DotaDashboard.Models.Entities;
using DotaDashboard.Services.Implementation;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Match = DotaDashboard.Models.Entities.Match;

namespace DotaDashboard.Tests.Services;

public class AggregateStatsServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly AggregateStatsService _service;

    public AggregateStatsServiceTests()
    {
        _context = GetInMemoryDbContext();
        var logger = new Mock<ILogger<AggregateStatsService>>();
        _service = new AggregateStatsService(_context, logger.Object);
    }

    private static ApplicationDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
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
        var result = await _service.GetTopHeroesByWinRateAsync(3);

        // Assert
        result.Count.Should().Be(3);
        result[0].Name.Should().Be("Hero1");
        result[0].WinRate.Should().Be(90);
        result[1].Name.Should().Be("Hero3");
        result[2].Name.Should().Be("Hero2");
    }

    [Fact]
    public async Task GetTopPlayersByKDA_ReturnsCorrectOrder()
    {
        await _context.Players.AddRangeAsync(
            new Player { PlayerId = 1, Name = "Player1", TotalKills = 100, TotalDeaths = 40, TotalAssists = 50, TotalMatches = 10, LastUpdated = DateTime.UtcNow }, // KDA = 15
            new Player { PlayerId = 2, Name = "Player2", TotalKills = 50, TotalDeaths = 25, TotalAssists = 25, TotalMatches = 10, LastUpdated = DateTime.UtcNow },  // KDA = 3
            new Player { PlayerId = 3, Name = "Player3", TotalKills = 80, TotalDeaths = 20, TotalAssists = 40, TotalMatches = 10, LastUpdated = DateTime.UtcNow }   // KDA = 6
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTopPlayersByKdaAsync(3);

        // Assert
        result.Count.Should().Be(3);
        result[1].Name.Should().Be("Player1");
        result[1].Kda.Should().BeApproximately(3.75, 0.01);
        result[0].Name.Should().Be("Player3");
        result[0].Kda.Should().BeApproximately(6, 0.01);
        result[2].Name.Should().Be("Player2");
        result[2].Kda.Should().BeApproximately(3, 0.01);
    }

    [Fact]
    public async Task GetTotalMatches_ReturnsCorrectCount()
    {
        await _context.Matches.AddRangeAsync(
            new Match { MatchId = 1, StartTime = DateTime.UtcNow, Duration = 1800, WinnerRadiant = true, ProcessedAt = DateTime.UtcNow },
            new Match { MatchId = 2, StartTime = DateTime.UtcNow, Duration = 2000, WinnerRadiant = false, ProcessedAt = DateTime.UtcNow },
            new Match { MatchId = 3, StartTime = DateTime.UtcNow, Duration = 1500, WinnerRadiant = true, ProcessedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTotalMatchesAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task GetActiveJobCount_ReturnsOnlyPendingAndRunningJobs()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var logger = new Mock<ILogger<AggregateStatsService>>();
        var service = new AggregateStatsService(context, logger.Object);

        context.Jobs.AddRange(
            new Job { Type = "IngestMatches", Status = JobStatus.Pending, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Job { Type = "IngestMatches", Status = JobStatus.Running, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Job { Type = "IngestMatches", Status = JobStatus.Done, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Job { Type = "IngestMatches", Status = JobStatus.Failed, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await service.GetActiveJobCountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetTopHeroes_FiltersOutLowPickCounts()
    {
        _context.Heroes.AddRange(
            new Hero { HeroId = 1, Name = "PopularHero", TotalPicks = 20, TotalWins = 18, LastUpdated = DateTime.UtcNow },
            new Hero { HeroId = 2, Name = "UnpopularHero", TotalPicks = 5, TotalWins = 5, LastUpdated = DateTime.UtcNow } // Below min picks
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetTopHeroesByWinRateAsync(5, 10);

        // Assert
        result.Count.Should().Be(1);
        result[0].Name.Should().Be("PopularHero");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
