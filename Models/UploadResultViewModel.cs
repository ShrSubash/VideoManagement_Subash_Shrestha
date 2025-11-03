namespace VideoManagementApp.Models
{
    /// <summary>
    /// View model for upload operation results
    /// </summary>
    public class UploadResultViewModel
    {
        /// <summary>
        /// Gets or sets whether the upload operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the message describing the result
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of files successfully uploaded
        /// </summary>  
        public int FilesUploaded { get; set; }

        /// <summary>
        /// Gets or sets the list of uploaded file names
        /// </summary>
        public List<string> UploadedFiles { get; set; } = new List<string>();
    }
}
