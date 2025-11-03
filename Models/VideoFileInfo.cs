namespace VideoManagementApp.Models
{
    /// <summary>
    /// Represents metadata for a video file in the media catalogue
    /// </summary>
    public class VideoFileInfo
    {
        /// <summary>
        /// Gets or sets the filename including extension
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }

        /// <summary>
        /// Gets the file size formatted as human-readable string (KB, MB, GB)
        /// </summary>
        public string FileSizeFormatted 
        {
            get
            {
                if (FileSizeBytes >= 1073741824) // GB
                    return $"{FileSizeBytes / 1073741824.0:F2} GB";
                if (FileSizeBytes >= 1048576) // MB
                    return $"{FileSizeBytes / 1048576.0:F2} MB";
                if (FileSizeBytes >= 1024) // KB
                    return $"{FileSizeBytes / 1024.0:F2} KB";
                
                return $"{FileSizeBytes} bytes";
            }
        }

        /// <summary>
        /// Gets the relative URL path for accessing this video file
        /// </summary>
        public string VideoUrl => $"/media/{FileName}";
    }
}
