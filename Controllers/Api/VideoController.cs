using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using VideoManagementApp.Models;
using VideoManagementApp.Services;

namespace VideoManagementApp.Controllers.Api
{
    /// <summary>
    /// API Controller for video file upload operations
    /// Route: /api/video
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService _videoService;
        private readonly ILogger<VideoController> _logger;
          
        /// <summary>
        /// Initializes a new instance of VideoController with dependency injection
        /// </summary>
        /// <param name="videoService">Video service for file operations</param>
        /// <param name="logger">Logger for tracking requests and errors</param>
        public VideoController(IVideoService videoService, ILogger<VideoController> logger)
        {
            _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Uploads one or more MP4 video files to the server
        /// POST: api/video/upload
        /// </summary>
        /// <param name="files">Collection of files from multipart/form-data request</param>
        /// <returns>UploadResultViewModel indicating success or failure</returns>
        [HttpPost("upload")]
        [RequestSizeLimit(209715200)] // 200 MB limit enforced at endpoint level
        [DisableRequestSizeLimit] // Disable the default limit, use custom limit above
        public async Task<IActionResult> Upload(IFormFileCollection files)
        {
            try
            {
                _logger.LogInformation("Received upload request with {Count} files", files?.Count ?? 0);

                // Validate request has files
                if (files == null || files.Count == 0)
                {
                    _logger.LogWarning("Upload request received with no files");
                    return BadRequest(new UploadResultViewModel
                    {
                        Success = false,
                        Message = "No files were provided for upload."
                    });
                }

                // Check content type
                if (!Request.HasFormContentType)
                {
                    _logger.LogWarning("Invalid content type for upload request");
                    return BadRequest(new UploadResultViewModel
                    {
                        Success = false,
                        Message = "Invalid content type. Expected multipart/form-data."
                    });
                }

                // Validate all files before processing
                foreach (var file in files)
                {
                    // Check file extension
                    if (!_videoService.ValidateFileExtension(file.FileName))
                    {
                        _logger.LogWarning("Invalid file extension: {FileName}", file.FileName);
                        return BadRequest(new UploadResultViewModel
                        {
                            Success = false,
                            Message = $"Only MP4 files are allowed. Invalid file: {file.FileName}"
                        });
                    }

                    // Check file size
                    if (!_videoService.ValidateFileSize(file.Length))
                    {
                        _logger.LogWarning("File size exceeds limit: {FileName} ({Size} bytes)", 
                            file.FileName, file.Length);
                        return StatusCode(StatusCodes.Status413PayloadTooLarge, new UploadResultViewModel
                        {
                            Success = false,
                            Message = $"File {file.FileName} exceeds the maximum size of 200 MB."
                        });
                    }
                }

                // Process upload using service layer
                var result = await _videoService.SaveVideosAsync(files);

                if (result.Success)
                {
                    _logger.LogInformation("Upload successful: {Count} files uploaded", result.FilesUploaded);
                    return Ok(result);
                }
                else
                {
                    _logger.LogWarning("Upload failed: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during upload processing");
                return StatusCode(StatusCodes.Status500InternalServerError, new UploadResultViewModel
                {
                    Success = false,
                    Message = "An unexpected error occurred during upload. Please try again later."
                });
            }
        }

        /// <summary>
        /// Gets all available video files in the catalogue
        /// GET: api/video/catalogue
        /// </summary>
        /// <returns>List of VideoFileInfo objects</returns>
        [HttpGet("catalogue")]
        public async Task<IActionResult> GetCatalogue()
        {
            try
            {
                _logger.LogInformation("Retrieving video catalogue");
                var videos = await _videoService.GetAllVideosAsync();
                return Ok(videos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving video catalogue");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    "An error occurred while retrieving the video catalogue.");
            }
        }
    }
}
