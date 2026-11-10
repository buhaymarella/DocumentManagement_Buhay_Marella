using DocumentManagementSystem.Data;
using DocumentManagementSystem.Entity;

namespace DocumentManagementSystem.DTOs
{
    public record DocumentResponse(
    int Id,
    Guid DocumentId,
    string OriginalFileName,
    string StoredFileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAtUtc)
    {
        public static DocumentResponse From(Document d) =>
            new(d.Id, d.DocumentId, d.OriginalFileName, d.StoredFileName, d.ContentType, d.SizeBytes, d.UploadedAtUtc);
    }
}
