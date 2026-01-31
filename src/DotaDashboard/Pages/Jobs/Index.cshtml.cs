using DotaDashboard.Models.Entities;
using DotaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotaDashboard.Pages.Jobs
{
    public class IndexModel(IJobService jobService, ILogger<IndexModel> logger) : PageModel
    {
        public List<Job> Jobs { get; set; } = new();
        public string? StatusFilter { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }

        /// <summary>
        /// Gets the list of jobs with optional status filtering and pagination.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task OnGetAsync(string? status = null, int page = 1)
        {
            try
            {
                StatusFilter = status;
                CurrentPage = page;

                // Get all jobs with pagination
                Jobs = await jobService.GetJobsAsync(page);

                // Filter by status if specified
                if (!string.IsNullOrEmpty(status))
                {
                    Jobs = Jobs.Where(j => j.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                logger.LogInformation("Loaded {Count} jobs for page {Page}", Jobs.Count, page);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading jobs");
            }
        }
        /// <summary>
        /// Retries a failed job by its ID.
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostRetryJobAsync(int jobId)
        {
            try
            {
                logger.LogInformation("Retrying job {JobId}", jobId);

                await jobService.RetryJobAsync(jobId);

                TempData["Success"] = $"Job {jobId} has been queued for retry!";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error retrying job {JobId}", jobId);
                TempData["Error"] = $"Failed to retry job: {ex.Message}";
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Creates a new job of the specified type.
        /// </summary>
        /// <param name="jobType"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnPostCreateJobAsync(string jobType)
        {
            try
            {
                logger.LogInformation("Creating job of type {JobType}", jobType);

                await jobService.CreateJobAsync(jobType);

                TempData["Success"] = $"Job '{jobType}' created successfully!";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating job");
                TempData["Error"] = $"Failed to create job: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}
