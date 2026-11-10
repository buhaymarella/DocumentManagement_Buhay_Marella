using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace DocumentManagementSystem.Tests;

public class DocumentApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public DocumentApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ---------- Helpers ----------

    private static MultipartFormDataContent BuildUpload(string fileName, byte[] bytes)
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new("text/plain");
        var form = new MultipartFormDataContent();
        form.Add(file, "file", fileName);
        return form;
    }

    private record UploadResult(int Id, string StoredFileName);

    private async Task<UploadResult> UploadAsync(string fileName = "hello.txt", string text = "hello world")
    {
        var response = await _client.PostAsync("/api/documents",
            BuildUpload(fileName, Encoding.UTF8.GetBytes(text)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var id = FindProperty(doc.RootElement, "id")!.Value.GetInt32();
        var stored = FindProperty(doc.RootElement, "storedFileName")!.Value.GetString()!;
        return new UploadResult(id, stored);
    }

    // Finds a property by name anywhere in the JSON, whatever the Result wrapper looks like.
    private static JsonElement? FindProperty(JsonElement el, string name)
    {
        if (el.ValueKind == JsonValueKind.Object)
            foreach (var p in el.EnumerateObject())
            {
                if (string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase)) return p.Value;
                var found = FindProperty(p.Value, name);
                if (found is not null) return found;
            }
        return null;
    }

    // ---------- Required tests ----------

    [Fact]
    public async Task Upload_ValidFile_ReturnsCreated_AndStoresFile()
    {
        var up = await UploadAsync();
        Assert.True(up.Id > 0);
        Assert.True(File.Exists(Path.Combine(_factory.StoragePath, up.StoredFileName)));
    }

    [Fact]
    public async Task GetById_ExistingDocument_ReturnsOk()
    {
        var up = await UploadAsync();
        var response = await _client.GetAsync($"/api/documents/{up.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/documents/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Upload_OverSizeLimit_ReturnsPayloadTooLarge()
    {
        var big = new byte[11 * 1024 * 1024 + 1]; // just over the 11 MB request limit
        var response = await _client.PostAsync("/api/documents", BuildUpload("big.txt", big));
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingDocument_ReturnsNoContent_ThenNotFound()
    {
        var up = await UploadAsync();

        var delete = await _client.DeleteAsync($"/api/documents/{up.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);

        var get = await _client.GetAsync($"/api/documents/{up.Id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        Assert.False(File.Exists(Path.Combine(_factory.StoragePath, up.StoredFileName)));
    }

    // ---------- Additional tests ----------

    [Fact]
    public async Task Upload_EmptyFile_ReturnsBadRequest()
    {
        var response = await _client.PostAsync("/api/documents",
            BuildUpload("empty.txt", Array.Empty<byte>()));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_UnsupportedExtension_ReturnsUnsupportedMediaType()
    {
        var response = await _client.PostAsync("/api/documents",
            BuildUpload("malware.exe", Encoding.UTF8.GetBytes("x")));
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, response.StatusCode);
    }

    [Fact]
    public async Task Upload_FileBetween10And11MB_ReturnsPayloadTooLarge()
    {
        var big = new byte[10 * 1024 * 1024 + 1]; 
        var response = await _client.PostAsync("/api/documents", BuildUpload("big.txt", big));
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task Upload_PathTraversalFileName_IsStoredSafely()
    {
        var up = await UploadAsync("../../evil.txt");
        Assert.DoesNotContain("..", up.StoredFileName);
        Assert.True(File.Exists(Path.Combine(_factory.StoragePath, up.StoredFileName)));
    }

    [Fact]
    public async Task Download_ExistingDocument_ReturnsSameBytes()
    {
        var up = await UploadAsync(text: "hello world");
        var response = await _client.GetAsync($"/api/documents/{up.Id}/download");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("hello world", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Download_FileMissingFromDisk_ReturnsNotFound()
    {
        var up = await UploadAsync();
        File.Delete(Path.Combine(_factory.StoragePath, up.StoredFileName));

        var response = await _client.GetAsync($"/api/documents/{up.Id}/download");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}