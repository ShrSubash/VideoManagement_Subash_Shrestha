/**
 *  Client-Side Controller
 * Handles view switching, file uploads, and video playback
 */
class VideoManager {
    constructor() {
        // DOM element references
        this.elements = {
            catalogueViewBtn: document.getElementById('catalogueViewBtn'),
            uploadViewBtn: document.getElementById('uploadViewBtn'),
            catalogueView: document.getElementById('catalogueView'),
            uploadView: document.getElementById('uploadView'),
            uploadForm: document.getElementById('uploadForm'),
            videoFiles: document.getElementById('videoFiles'),
            uploadBtn: document.getElementById('uploadBtn'),
            cancelUploadBtn: document.getElementById('cancelUploadBtn'),
            uploadProgress: document.getElementById('uploadProgress'),
            alertContainer: document.getElementById('alertContainer'),
            videoPlayerSection: document.getElementById('videoPlayerSection'),
            videoPlayer: document.getElementById('videoPlayer'),
            currentVideoName: document.getElementById('currentVideoName'),
            closeVideoBtn: document.getElementById('closeVideoBtn'),
            selectedFilesPreview: document.getElementById('selectedFilesPreview'),
            filesList: document.getElementById('filesList'),
            videoTable: document.getElementById('videoTable')
        };

        // Configuration
        this.config = {
            uploadApiUrl: '/api/video/upload',
            maxFileSize: 209715200, // 200 MB in bytes
            allowedExtension: '.mp4'
        };

        // State
        this.state = {
            currentView: 'catalogue',
            isUploading: false,
            currentVideoRow: null
        };

        // Initialize event listeners
        this.initializeEventListeners();
    }

    /**
     * Initialize all event listeners
     */
    initializeEventListeners() {
        // View toggle buttons
        this.elements.catalogueViewBtn.addEventListener('click', () => this.showView('catalogue'));
        this.elements.uploadViewBtn.addEventListener('click', () => this.showView('upload'));

        // Upload form events
        this.elements.uploadForm.addEventListener('submit', (e) => this.handleUpload(e));
        this.elements.cancelUploadBtn.addEventListener('click', () => this.showView('catalogue'));
        this.elements.videoFiles.addEventListener('change', (e) => this.handleFileSelection(e));

        // Video player events
        this.elements.closeVideoBtn.addEventListener('click', () => this.closeVideoPlayer());

        // Video table row clicks
        if (this.elements.videoTable) {
            const videoRows = this.elements.videoTable.querySelectorAll('.video-row');
            videoRows.forEach(row => {
                row.addEventListener('click', () => this.playVideo(row));
            });
        }
    }

    /**
     * Show specified view and update UI accordingly
     * @param {string} viewName - 'catalogue' or 'upload'
     */
    showView(viewName) {
        this.state.currentView = viewName;

        if (viewName === 'catalogue') {
            // Show catalogue, hide upload
            this.elements.catalogueView.style.display = 'block';
            this.elements.uploadView.style.display = 'none';
            
            // Update button states
            this.elements.catalogueViewBtn.classList.add('active');
            this.elements.uploadViewBtn.classList.remove('active');
            
            // Add fade-in animation
            this.elements.catalogueView.classList.add('fade-in');
        } else if (viewName === 'upload') {
            // Show upload, hide catalogue
            this.elements.catalogueView.style.display = 'none';
            this.elements.uploadView.style.display = 'block';
            
            // Update button states
            this.elements.uploadViewBtn.classList.add('active');
            this.elements.catalogueViewBtn.classList.remove('active');
            
            // Add fade-in animation
            this.elements.uploadView.classList.add('fade-in');
            
            // Reset upload form
            this.resetUploadForm();
        }

        // Clear any existing alerts
        this.clearAlerts();
    }

    /**
     * Handle file selection and display preview
     * @param {Event} event - File input change event
     */
    handleFileSelection(event) {
        const files = event.target.files;
        
        if (files.length === 0) {
            this.elements.selectedFilesPreview.style.display = 'none';
            return;
        }

        // Clear previous file list
        this.elements.filesList.innerHTML = '';

        // Validate and display selected files
        let hasInvalidFiles = false;
        Array.from(files).forEach(file => {
            const listItem = document.createElement('li');
            listItem.className = 'list-group-item';

            const fileName = document.createElement('span');
            fileName.textContent = file.name;

            const fileSize = document.createElement('span');
            fileSize.className = 'badge bg-secondary';
            fileSize.textContent = this.formatFileSize(file.size);

            // Validate file
            if (!this.validateFile(file)) {
                listItem.classList.add('list-group-item-danger');
                const errorIcon = document.createElement('i');
                errorIcon.className = 'bi bi-exclamation-circle text-danger ms-2';
                fileName.appendChild(errorIcon);
                hasInvalidFiles = true;
            }

            listItem.appendChild(fileName);
            listItem.appendChild(fileSize);
            this.elements.filesList.appendChild(listItem);
        });

        // Show preview
        this.elements.selectedFilesPreview.style.display = 'block';

        // Disable upload button if invalid files detected
        this.elements.uploadBtn.disabled = hasInvalidFiles;

        if (hasInvalidFiles) {
            this.showAlert('Some files are invalid. Only MP4 files up to 200MB are allowed.', 'warning');
        }
    }

    /**
     * Validate individual file
     * @param {File} file - File to validate
     * @returns {boolean} - True if valid, false otherwise
     */
    validateFile(file) {
        // Check extension
        const extension = '.' + file.name.split('.').pop().toLowerCase();
        if (extension !== this.config.allowedExtension) {
            return false;
        }

        // Check file size
        if (file.size > this.config.maxFileSize || file.size === 0) {
            return false;
        }

        return true;
    }

    /**
     * Format file size to human-readable format
     * @param {number} bytes - File size in bytes
     * @returns {string} - Formatted file size
     */
    formatFileSize(bytes) {
        if (bytes >= 1073741824) {
            return (bytes / 1073741824).toFixed(2) + ' GB';
        } else if (bytes >= 1048576) {
            return (bytes / 1048576).toFixed(2) + ' MB';
        } else if (bytes >= 1024) {
            return (bytes / 1024).toFixed(2) + ' KB';
        }
        return bytes + ' bytes';
    }

    /**
     * Handle form submission and file upload
     * @param {Event} event - Form submit event
     */
    async handleUpload(event) {
        event.preventDefault();

        // Prevent multiple simultaneous uploads
        if (this.state.isUploading) {
            return;
        }

        const files = this.elements.videoFiles.files;

        // Validate files exist
        if (files.length === 0) {
            this.showAlert('Please select at least one file to upload.', 'warning');
            return;
        }

        // Client-side validation
        for (let i = 0; i < files.length; i++) {
            if (!this.validateFile(files[i])) {
                this.showAlert('Invalid file detected. Please check your selections.', 'danger');
                return;
            }
        }

        try {
            // Set uploading state
            this.state.isUploading = true;
            this.elements.uploadBtn.disabled = true;
            this.elements.uploadProgress.style.display = 'block';
            this.clearAlerts();

            // Prepare form data
            const formData = new FormData();
            for (let i = 0; i < files.length; i++) {
                formData.append('files', files[i]);
            }

            // Send upload request to API
            const response = await fetch(this.config.uploadApiUrl, {
                method: 'POST',
                body: formData
            });

            const result = await response.json();

            // Hide progress
            this.elements.uploadProgress.style.display = 'none';

            if (response.ok && result.success) {
                // Upload successful
                this.showAlert(result.message, 'success');
                
                // Wait briefly to show success message, then reload catalogue
                setTimeout(() => {
                    window.location.reload(); // Reload to show updated catalogue
                }, 1500);
            } else {
                // Upload failed
                this.showAlert(result.message || 'Upload failed. Please try again.', 'danger');
                this.state.isUploading = false;
                this.elements.uploadBtn.disabled = false;
            }
        } catch (error) {
            console.error('Upload error:', error);
            this.elements.uploadProgress.style.display = 'none';
            this.showAlert('An error occurred during upload. Please try again.', 'danger');
            this.state.isUploading = false;
            this.elements.uploadBtn.disabled = false;
        }
    }

    /**
     * Play selected video
     * @param {HTMLElement} row - Table row element containing video data
     */
    playVideo(row) {
        const videoUrl = row.getAttribute('data-video-url');
        const videoName = row.getAttribute('data-video-name');
        const videoSize = row.getAttribute('data-video-size');

        // Remove active class from ALL rows (defensive approach) - FIX FOR HIGHLIGHTING
        const allRows = document.querySelectorAll('.video-row');
        allRows.forEach(r => r.classList.remove('active'));

        // Set active class on current row
        row.classList.add('active');
        this.state.currentVideoRow = row;

        // Update video player
        this.elements.currentVideoName.textContent = videoName;

        // Update video size in the footer
        const videoSizeElement = document.getElementById('currentVideoSize');
        if (videoSizeElement) {
            videoSizeElement.textContent = videoSize;
        }

        this.elements.videoPlayer.src = videoUrl;
        this.elements.videoPlayer.load();

        // Show video player section
        this.elements.videoPlayerSection.style.display = 'block';
        this.elements.videoPlayerSection.classList.add('fade-in');

        // Start playback
        this.elements.videoPlayer.play().catch(error => {
            console.error('Error playing video:', error);
            this.showAlert('Unable to play video. Please try another file.', 'warning');
        });

        // Scroll video player into view
        this.elements.videoPlayerSection.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }
    /**
     * Close video player
     */
    closeVideoPlayer() {
        // Stop playback
        this.elements.videoPlayer.pause();
        this.elements.videoPlayer.src = '';
        
        // Hide player section
        this.elements.videoPlayerSection.style.display = 'none';
        
        // Remove active class from row
        if (this.state.currentVideoRow) {
            this.state.currentVideoRow.classList.remove('active');
            this.state.currentVideoRow = null;
        }
    }

    /**
     * Reset upload form to initial state
     */
    resetUploadForm() {
        this.elements.uploadForm.reset();
        this.elements.selectedFilesPreview.style.display = 'none';
        this.elements.filesList.innerHTML = '';
        this.elements.uploadBtn.disabled = false;
        this.elements.uploadProgress.style.display = 'none';
        this.state.isUploading = false;
    }

    /**
     * Display alert message
     * @param {string} message - Alert message
     * @param {string} type - Alert type (success, danger, warning, info)
     */
    showAlert(message, type) {
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.role = 'alert';
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        `;
        
        this.elements.alertContainer.appendChild(alert);

        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            alert.classList.remove('show');
            setTimeout(() => alert.remove(), 150);
        }, 5000);
    }

    /**
     * Clear all alerts
     */
    clearAlerts() {
        this.elements.alertContainer.innerHTML = '';
    }
}

// Export for use in other scripts if needed
if (typeof module !== 'undefined' && module.exports) {
    module.exports = VideoManager;
}
