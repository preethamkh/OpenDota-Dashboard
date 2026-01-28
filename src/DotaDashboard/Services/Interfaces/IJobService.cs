using DotaDashboard.Models.Entities;

namespace DotaDashboard.Services.Interfaces;

/// <summary>
/// Service for managing background jobs
/// </summary>
public interface IJobService
{
    /// <summary>
    /// Create a new job and publish to queue
    /// </summary>
    Task<Job> CreateJobAsync(string jobType, string? target = null);

    /// <summary>
    /// Update job status
    /// </summary>
    Task UpdateJobStatusAsync(int jobId, string status, string? error = null);
    /// <summary>
    /// Update / Reporting how much work has been done (progress), not the job's state.
    /// </summary>
    Task UpdateJobProgressAsync(int jobId, int matchesProcessed);

    /// <summary>
    /// Mark job as failed
    /// </summary>
    Task FailJobAsync(int jobId, string error);

    /// <summary>
    /// Get a job by ID
    /// </summary>
    Task<Job?> GetJobByIdAsync(int jobId);

    /// <summary>
    /// Get all jobs with pagination
    /// </summary>
    Task<List<Job>> GetJobsAsync(int page = 1, int pageSize = 20);

    /// <summary>
    /// Get pending jobs
    /// </summary>
    Task<List<Job>> GetPendingJobsAsync();

    /// <summary>
    /// Get running jobs
    /// </summary>
    Task<List<Job>> GetRunningJobsAsync();

    /// <summary>
    /// Retry a failed job
    /// </summary>
    Task<Job> RetryJobAsync(int jobId);

    /// <summary>
    /// Get active job count(Pending + Running
    /// </summary>
    Task<int> GetActiveJobCountAsync();
}
