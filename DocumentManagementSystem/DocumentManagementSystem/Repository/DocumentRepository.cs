using DocumentManagementSystem.Command;
using DocumentManagementSystem.Data;
using DocumentManagementSystem.DTOs;
using DocumentManagementSystem.Entity;
using DocumentManagementSystem.Options;
using DocumentManagementSystem.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography.X509Certificates;

namespace DocumentManagementSystem.Repository
{
    public class DocumentRepository : IDocumentRepository<Document>
    {
        private readonly AppDbContext _context;
        private readonly IFileStorage _storage;
        private readonly UploadOptions _upload;
        private readonly ILogger<DocumentRepository> _logger;

        public DocumentRepository(AppDbContext context, IFileStorage storage,
            IOptions<UploadOptions> upload, ILogger<DocumentRepository> logger)
        {
            _context = context;
            _storage = storage;
            _upload = upload.Value;
            _logger = logger;
        }

        public async Task<Result> UploadFileAsync(UploadFileCommand cmd, CancellationToken ct)
        {
            var result = new Result();
          
                if (cmd.File is null || cmd.File.Length == 0)
                    throw new InvalidUploadException("A non-empty file is required.");

                if (cmd.File.Length > _upload.MaxFileSizeBytes)
                    throw new FileTooLargeException(
                        $"File exceeds the maximum size of {_upload.MaxFileSizeBytes / (1024 * 1024)} MB.");

                var originalName = Path.GetFileName(cmd.File.FileName);

                if (string.IsNullOrWhiteSpace(originalName))
                    throw new InvalidUploadException("A valid file name is required.");

                if (originalName.Length > 255)
                    originalName = originalName[^255..];

                var ext = Path.GetExtension(originalName).ToLowerInvariant();
                if (!_upload.AllowedExtensions.Contains(ext))
                    throw new UnsupportedFileTypeException(
                        $"File type '{ext}' is not supported. Allowed: {string.Join(", ", _upload.AllowedExtensions)}");


                var id = Guid.NewGuid();
                var storedName = $"{id:N}{ext}";

                await using (var stream = cmd.File.OpenReadStream())
                {
                    await _storage.SaveAsync(storedName, stream, ct);
                }

                var doc = new Document
                {
                    DocumentId = id,
                    OriginalFileName = originalName,
                    StoredFileName = storedName,
                    ContentType = string.IsNullOrWhiteSpace(cmd.File.ContentType)
                        ? "application/octet-stream" : cmd.File.ContentType,
                    SizeBytes = cmd.File.Length,
                    UploadedAtUtc = DateTime.UtcNow
                };

                _context.Add(doc);
                try
                {
                    await _context.SaveChangesAsync(ct);
                }
                catch
                {
                    try { await _storage.DeleteAsync(storedName); }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to clean up stored file {StoredName} after DB failure.", storedName);
                    }
                    throw;
                }

                return result.Success("Successfully Saved Document", DocumentResponse.From(doc));
        }

        public async Task<Result> GetListDocumentsAsync(CancellationToken ct, GetListDocQuery qry)
        {
            var result = new Result();
            try
            {
                int pageSize = qry.PageSize ?? 10;
                int pageIndex = qry.PageIndex ?? 0;

                var query = await _context.Documents.AsNoTracking()
                    .OrderByDescending(d => d.UploadedAtUtc)
                    .ToListAsync(ct);

                var totalRecords = query.Count();

                var docQry = query
                    .OrderBy(query => query.Id)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var data = docQry.Select(x => new DocumentResponse(
                    x.Id,
                    x.DocumentId,
                    x.OriginalFileName,
                    x.StoredFileName,
                    x.ContentType,
                    x.SizeBytes,
                    x.UploadedAtUtc
                ));

                return result.Success("Successfully Fetched Documents", data);
            }
            catch (Exception ex)
            {
                return result.Fail(ex.Message);
            }
        }

        public async Task<Result> GetDocumentByIdAsync(GetDocumentByIdQuery qry, CancellationToken ct)
        {
            var result = new Result();
            try
            {
                if (qry.Id == 0 || qry.Id == null)
                    return result.Fail("Id is required");

                var docQry = await _context.Documents
                    .Where(x => x.Id == qry.Id)
                    .FirstOrDefaultAsync()
                    .ConfigureAwait(false);

                if (docQry == null)
                    return null;

                var data = new DocumentResponse(
                   docQry.Id,
                   docQry.DocumentId,
                   docQry.OriginalFileName,
                   docQry.StoredFileName,
                   docQry.ContentType,
                   docQry.SizeBytes,
                   docQry.UploadedAtUtc
               );

                return result.Success("Fetch Data Successfuly!", data);
            }catch (Exception ex)
            {
                return result.Fail(ex.Message);
            }
        }

        public async Task<(Stream Stream, string ContentType, string FileName)?> DownloadDocAsync(DownloadFileQuery qry, CancellationToken ct)
        {
            var doc = await _context.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == qry.Id, ct);
            if (doc is null) return null;

            try
            {
                var stream = _storage.OpenRead(doc.StoredFileName);
                return (stream, doc.ContentType, doc.OriginalFileName);
            }
            catch (FileNotFoundException)
            {
                _logger.LogError("Record {Id} exists but file {StoredName} is missing from storage.",
                    qry.Id, doc.StoredFileName);
                return null;
            }
        }

        public async Task<Result> DeleteDocAsync(DeleteDocCommand cmd, CancellationToken ct)
        {
            var res = new Result();

            if (cmd.Id == 0 || cmd.Id == null)
                return res.Fail("Id is required");

            var docQry = await _context.Documents
                .Where(x => x.Id == cmd.Id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            if (docQry == null)
                return null;

            _context.Documents.Remove(docQry);
            await _context.SaveChangesAsync(ct);

            try
            {
                await _storage.DeleteAsync(docQry.StoredFileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Record {Id} deleted but file {StoredName} could not be removed.",
                    cmd.Id, docQry.StoredFileName);
            }

            return res.Success("Successfully Deleted");

        }
    }
}
