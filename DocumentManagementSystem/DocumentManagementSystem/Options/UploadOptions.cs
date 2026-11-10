namespace DocumentManagementSystem.Options
{
    public class UploadOptions
    {
        public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
        public string[] AllowedExtensions { get; set; } =
            { ".pdf", ".txt", ".csv", ".png", ".jpg", ".jpeg", ".docx", ".xlsx" };
    }
}
