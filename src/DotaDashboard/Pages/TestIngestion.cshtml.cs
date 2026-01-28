using DotaDashboard.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotaDashboard.Pages
{
    public class TestIngestionModel(
        IDataIngestionService dataIngestionService,
        ILogger<TestIngestionModel> logger)
        : PageModel
    {
        [BindProperty]
        public string Message { get; set; } = string.Empty;

        [BindProperty]
        public int HeroesIngested { get; set; }

        [BindProperty]
        public int MatchesIngested { get; set; }

        [BindProperty]
        public bool IsProcessing { get; set; }

        public void OnGet()
        {
            Message = "Ready to ingest data from OpenDota API";
        }

        public async Task<IActionResult> OnPostIngestHeroesAsync()
        {
            try
            {
                // todo: re. isProcessing, need client side JS to make this work effectively
                IsProcessing = true;
                logger.LogInformation("Starting hero ingestion via test page");

                HeroesIngested = await dataIngestionService.IngestHeroesAsync();

                Message = $"Successfully ingested {HeroesIngested} heroes!";
                logger.LogInformation("Hero ingestion completed. Count: {Count}", HeroesIngested);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during hero ingestion");
                Message = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostIngestMatchesAsync()
        {
            try
            {
                // todo: re. isProcessing, need client side JS to make this work effectively
                IsProcessing = true;
                logger.LogInformation("Starting match ingestion via test page");

                // First ensure heroes are loaded
                var heroCount = await dataIngestionService.IngestHeroesAsync();
                if (heroCount > 0)
                {
                    logger.LogInformation("Loaded {Count} heroes before match ingestion", heroCount);
                }

                // Ingest matches (limited to 1 for testing - as each match requires a match detail fetch (secondary call))
                // Better quality (non bot ) matches from pro matches endpoint
                // MatchesIngested = await dataIngestionService.IngestPublicMatchesAsync(200);
                MatchesIngested = await dataIngestionService.IngestProMatchesAsync(1);

                Message = $"Successfully ingested {MatchesIngested} matches! (Limited to 1 for testing)";
                logger.LogInformation("Match ingestion completed. Count: {Count}", MatchesIngested);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during match ingestion");
                Message = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }

            return Page();
        }
    }
}
