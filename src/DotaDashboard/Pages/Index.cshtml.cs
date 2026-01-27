using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotaDashboard.Pages
{
    public class IndexModel(ILogger<IndexModel> logger) : PageModel
    {
        public void OnGet()
        {
            logger.LogInformation("Home page accessed");
        }
    }
}
