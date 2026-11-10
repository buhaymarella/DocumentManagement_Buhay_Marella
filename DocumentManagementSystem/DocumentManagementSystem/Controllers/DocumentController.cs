using DocumentManagementSystem.Command;
using DocumentManagementSystem.DTOs;
using DocumentManagementSystem.Entity;
using DocumentManagementSystem.Repository;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagementSystem.Controllers
{

    [Route("api/documents")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        public readonly IDocumentRepository<Document> _documentRepository;
        private const long RequestLimitBytes = 11 * 1024 * 1024;
        public DocumentController(IDocumentRepository<Document> documentRepository)
        {
            _documentRepository = documentRepository;
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(RequestLimitBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = RequestLimitBytes)]
        [ProducesResponseType(typeof(DocumentResponse), StatusCodes.Status201Created)]
        public async Task<IActionResult> Upload([FromForm] UploadFileCommand cmd, CancellationToken ct)
        {
            var result = await _documentRepository.UploadFileAsync(cmd, ct).ConfigureAwait(false);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpGet]
        public async Task<ActionResult> GetListAsync([FromQuery] GetListDocQuery qry, CancellationToken ct)
        {
            return Ok(await _documentRepository.GetListDocumentsAsync(ct, qry));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var doc = await _documentRepository.GetDocumentByIdAsync(new GetDocumentByIdQuery { Id = id }, ct);
            return doc is null ? NotFound(Problem404("Document not found.")) : Ok(doc);
        }
        [HttpGet("{id:int}/download")]
        public async Task<IActionResult> Download(int id, CancellationToken ct)
        {
            var result = await _documentRepository.DownloadDocAsync(new DownloadFileQuery { Id = id }, ct);
            if (result is null)
                return NotFound(Problem404("Document or its stored file was not found."));

            var (stream, contentType, fileName) = result.Value;
            return File(stream, contentType, fileName);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var deleted = await _documentRepository.DeleteDocAsync(new DeleteDocCommand { Id = id }, ct);
            return deleted is null ? NotFound(Problem404("Document not found.")) : NoContent();
        }

        private ProblemDetails Problem404(string detail) => new()
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found",
            Detail = detail
        };
    }
}
