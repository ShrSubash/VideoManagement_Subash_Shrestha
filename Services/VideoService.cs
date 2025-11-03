using VideoManagementApp.Models;

namespace VideoManagementApp.Services
{
    /// <summary>
    /// Service implementation for video file management
    /// Follows Single Responsibility Principle - handles only video file operations
    /// </summary>
    public class VideoService : IVideoService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<VideoService> _logger;
        private const long MaxFileSizeBytes = 209715200; // 200 MB
        private const string MediaFolderName = "media";
        private const string AllowedExtension = ".mp4";
         
        /// <summary>
        /// Initializes a new instance of VideoService with dependency injection
        /// </summary>
        /// <param name="environment">Web host environment for path resolution</param>
        /// <param name="logger">Logger for tracking operations and errors</param>
        public VideoService(IWebHostEnvironment environment, ILogger<VideoService> logger)
        {
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the full path to the media directory
        /// </summary>
        private string MediaPath => Path.Combine(_environment.WebRootPath, MediaFolderName);

        /// <inheritdoc />
        public async Task<List<VideoFileInfo>> GetAllVideosAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all video files from media directory");

                // Ensure directory exists
                EnsureMediaDirectoryExists();

                var videoFiles = new List<VideoFileInfo>();
                var directoryInfo = new DirectoryInfo(MediaPath);

                // Get all MP4 files in the media directory
                var files = directoryInfo.GetFiles($"*{AllowedExtension}", SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                {
                    videoFiles.Add(new VideoFileInfo
                    {
                        FileName = file.Name,
                        FileSizeBytes = file.Length
                    });
                }

                _logger.LogInformation("Successfully retrieved {Count} video files", videoFiles.Count);
                return await Task.FromResult(videoFiles.OrderBy(v => v.FileName).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving video files");
                throw new InvalidOperationException("Failed to retrieve video files", ex);
            }
        }

        /// <inheritdoc />
        public async Task<UploadResultViewModel> SaveVideosAsync(IFormFileCollection files)
        {
            var result = new UploadResultViewModel();
            var uploadedFiles = new List<string>();

            try
            {
                // Validate input
                if (files == null || files.Count == 0)
                {
                    _logger.LogWarning("No files provided for upload");
                    result.Success = false;
                    result.Message = "No files were selected for upload.";
                    return result;
                }

                _logger.LogInformation("Starting upload process for {Count} files", files.Count);

                // Ensure directory exists
                EnsureMediaDirectoryExists();

                foreach (var file in files)
                {
                    // Validate file extension
                    if (!ValidateFileExtension(file.FileName))
                    {
                        _logger.LogWarning("Invalid file extension for file: {FileName}", file.FileName);
                        result.Success = false;
                        result.Message = $"Only MP4 files are allowed. Invalid file: {file.FileName}";
                        return result;
                    }

                    // Validate file size
                    if (!ValidateFileSize(file.Length))
                    {
                        _logger.LogWarning("File size exceeds limit for file: {FileName} ({Size} bytes)", 
                            file.FileName, file.Length);
                        result.Success = false;
                        result.Message = $"File {file.FileName} exceeds the maximum size of 200 MB.";
                        return result;
                    }

                    // Save file to media directory
                    var filePath = Path.Combine(MediaPath, file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await file.CopyToAsync(stream);
                    }

                    uploadedFiles.Add(file.FileName);
                    _logger.LogInformation("Successfully uploaded file: {FileName}", file.FileName);
                }

                result.Success = true;
                result.FilesUploaded = uploadedFiles.Count;
                result.UploadedFiles = uploadedFiles;
                result.Message = $"Successfully uploaded {uploadedFiles.Count} file(s).";

                _logger.LogInformation("Upload process completed successfully. Files uploaded: {Count}", uploadedFiles.Count);
                return result;
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "IO error occurred during file upload");
                result.Success = false;
                result.Message = "An error occurred while saving the files. Please try again.";
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during file upload");
                result.Success = false;
                result.Message = "An unexpected error occurred. Please contact support.";
                return result;
            }
        }

        /// <inheritdoc />
        public bool ValidateFileSize(long fileSize)
        {
            return fileSize > 0 && fileSize <= MaxFileSizeBytes;
        }

        /// <inheritdoc />
        public bool ValidateFileExtension(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var extension = Path.GetExtension(fileName);
            return extension.Equals(AllowedExtension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Ensures the media directory exists, creates it if not present
        /// </summary>
        private void EnsureMediaDirectoryExists()
        {
            if (!Directory.Exists(MediaPath))
            {
                Directory.CreateDirectory(MediaPath);
                _logger.LogInformation("Created media directory at: {Path}", MediaPath);
            }
        }
    }
}
