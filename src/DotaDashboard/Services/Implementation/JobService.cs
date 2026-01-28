using DotaDashboard.Data;
using DotaDashboard.Models.Entities;
using DotaDashboard.Services.Interfaces;

namespace DotaDashboard.Services.Implementation;

public class JobService(ApplicationDbContext context, ILogger<JobService> logger) : IJobService
{
    public async Task<Job> CreateJobAsync(string jobType, string? target = null)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateJobStatusAsync(int jobId, string status, string? error = null)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateJobProgressAsync(int jobId, int matchesProcessed)
    {
        throw new NotImplementedException();
    }

    public async Task FailJobAsync(int jobId, string error)
    {
        throw new NotImplementedException();
    }

    public async Task<Job?> GetJobByIdAsync(int jobId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Job>> GetJobsAsync(int page = 1, int pageSize = 20)
    {
        throw new NotImplementedException();
    }

    public async Task<List<Job>> GetPendingJobsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<List<Job>> GetRunningJobsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Job> RetryJobAsync(int jobId)
    {
        throw new NotImplementedException();
    }

    public async Task<int> GetActiveJobCountAsync()
    {
        throw new NotImplementedException();
    }
}
