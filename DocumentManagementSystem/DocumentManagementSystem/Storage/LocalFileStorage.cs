using DocumentManagementSystem.Options;
using Microsoft.Extensions.Options;

namespace DocumentManagementSystem.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _root;

    public LocalFileStorage(IOptions<StorageOptions> options, IHostEnvironment env)
    {
        _root = Path.GetFullPath(Path.Combine(env.ContentRootPath, options.Value.RootPath));
        Directory.CreateDirectory(_root);
    }

    public async Task SaveAsync(string storedName, Stream content, CancellationToken ct)
    {
        var path = ResolvePath(storedName);
        try
        {
            await using var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write,
                FileShare.None, 81920, useAsync: true);
            await content.CopyToAsync(fs, ct);
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }
    }

    public Stream OpenRead(string storedName)
    {
        var path = ResolvePath(storedName);
        if (!File.Exists(path))
            throw new FileNotFoundException("Stored file not found.", storedName);

        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
    }

    public Task DeleteAsync(string storedName)
    {
        var path = ResolvePath(storedName);
        File.Delete(path);
        return Task.CompletedTask;
    }

    public bool Exists(string storedName) => File.Exists(ResolvePath(storedName));

    private string ResolvePath(string storedName)
    {
        if (string.IsNullOrWhiteSpace(storedName) || storedName != Path.GetFileName(storedName))
            throw new InvalidOperationException("Invalid storage name.");

        var full = Path.GetFullPath(Path.Combine(_root, storedName));
        if (!full.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new InvalidOperationException("Invalid storage path.");

        return full;
    }
}