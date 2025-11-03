namespace VideoManagementApp.Models
{
    /// <summary>
    /// View model for error pages
    /// </summary>
    public class ErrorViewModel
    {
        /// <summary>
        /// Gets or sets the request ID for tracking
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets whether the RequestId should be shown
        /// </summary>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
} 
