using VideoManagementApp.Models;

namespace VideoManagementApp.Services
{
    /// <summary>
    /// Service interface for video file management operations
    /// Follows Interface Segregation Principle by defining clear contract
    /// </summary>
    public interface IVideoService
    {
        /// <summary>
        /// Retrieves all MP4 video files from the media directory
        /// </summary>
        /// <returns>List of video file information objects</returns>
        Task<List<VideoFileInfo>> GetAllVideosAsync();

        /// <summary>
        /// Saves uploaded video files to the media directory
        /// </summary>
        /// <param name="files">Collection of uploaded files</param>
        /// <returns>Result indicating success/failure and details</returns>
        Task<UploadResultViewModel> SaveVideosAsync(IFormFileCollection files);

        /// <summary>
        /// Validates file size against maximum allowed size
        /// </summary>
        /// <param name="fileSize">File size in bytes</param>
        /// <returns>True if file size is within limits, false otherwise</returns>
        bool ValidateFileSize(long fileSize);

        /// <summary>
        /// Validates file extension to ensure only MP4 files are processed
        /// </summary>
        /// <param name="fileName">Name of the file to validate</param>
        /// <returns>True if file has .mp4 extension, false otherwise</returns>
        bool ValidateFileExtension(string fileName);
    }
}
