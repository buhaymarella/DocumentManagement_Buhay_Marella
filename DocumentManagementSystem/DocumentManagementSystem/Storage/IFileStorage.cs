namespace DocumentManagementSystem.Storage
{
    public interface IFileStorage
    {
        Task SaveAsync(string storedName, Stream content, CancellationToken ct);

        /// <summary>Throws FileNotFoundException if the file is missing.</summary>
        Stream OpenRead(string storedName);

        /// <summary>Idempotent: does nothing if the file is already gone.</summary>
        Task DeleteAsync(string storedName);

        bool Exists(string storedName);
    }
}
