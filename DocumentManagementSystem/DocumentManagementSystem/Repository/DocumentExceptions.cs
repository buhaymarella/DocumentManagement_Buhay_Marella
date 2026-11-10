namespace DocumentManagementSystem.Repository
{
    public class InvalidUploadException : Exception
    {
        public InvalidUploadException(string message) : base(message) { }
    }

    public class FileTooLargeException : Exception
    {
        public FileTooLargeException(string message) : base(message) { }
    }

    public class UnsupportedFileTypeException : Exception
    {
        public UnsupportedFileTypeException(string message) : base(message) { }
    }
}
