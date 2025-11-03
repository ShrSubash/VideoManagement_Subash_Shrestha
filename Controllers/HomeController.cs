using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VideoManagementApp.Models;
using VideoManagementApp.Services;

namespace VideoManagementApp.Controllers
{
    /// <summary>
    /// MVC Controller for Home views
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVideoService _videoService;

        /// <summary>
        /// Initializes a new instance of HomeController with dependency injection
        /// </summary>
        /// <param name="logger">Logger for tracking requests</param>
        /// <param name="videoService">Video service for retrieving video catalogue</param>
        public HomeController(ILogger<HomeController> logger, IVideoService videoService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
        }

        /// <summary>
        /// Default Index action - displays video management interface
        /// GET: / or /Home/Index
        /// </summary>
        /// <returns>Index view with video catalogue</returns>
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Loading Index page");
                
                // Retrieve all videos to populate initial catalogue view
                var videos = await _videoService.GetAllVideosAsync();
                
                return View(videos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Index page");
                return View("Error", new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                });
            }
        }

        /// <summary>
        /// Error action for handling application errors
        /// GET: /Home/Error
        /// </summary>
        /// <returns>Error view</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
}
