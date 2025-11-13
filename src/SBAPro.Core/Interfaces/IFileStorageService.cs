namespace SBAPro.Core.Interfaces;

/// <summary>
/// Service for storing and retrieving files (floor plans, photos).
/// Allows for abstraction over different storage backends (local, cloud).
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    /// <param name="contentType">MIME type of the file.</param>
    /// <param name="fileData">File content as byte array.</param>
    /// <returns>A URL or identifier for accessing the file.</returns>
    Task<string> UploadFileAsync(string fileName, string contentType, byte[] fileData);

    /// <summary>
    /// Downloads a file from storage.
    /// </summary>
    /// <param name="fileId">The file identifier or URL.</param>
    /// <returns>The file content as byte array.</returns>
    Task<byte[]> DownloadFileAsync(string fileId);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="fileId">The file identifier or URL.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteFileAsync(string fileId);

    /// <summary>
    /// Checks if a file exists in storage.
    /// </summary>
    /// <param name="fileId">The file identifier or URL.</param>
    /// <returns>True if the file exists, otherwise false.</returns>
    Task<bool> FileExistsAsync(string fileId);
}
