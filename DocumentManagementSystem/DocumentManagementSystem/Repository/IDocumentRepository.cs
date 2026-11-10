using DocumentManagementSystem.Command;

namespace DocumentManagementSystem.Repository
{
    public interface IDocumentRepository<T> where T : class
    {
        Task<Result> UploadFileAsync(UploadFileCommand cmd, CancellationToken ct);
        Task<Result> GetListDocumentsAsync(CancellationToken ct, GetListDocQuery qry);
        Task<Result> GetDocumentByIdAsync(GetDocumentByIdQuery qry, CancellationToken ct);
        Task<(Stream Stream, string ContentType, string FileName)?> DownloadDocAsync(DownloadFileQuery qry, CancellationToken ct);
        Task<Result> DeleteDocAsync(DeleteDocCommand cmd, CancellationToken ct);
    }
}
