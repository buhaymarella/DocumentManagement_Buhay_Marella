namespace DocumentManagementSystem.Entity
{
    public class Document
    {
        public int Id { get; set; }
        public Guid DocumentId { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime UploadedAtUtc { get; set; }
    }
}
