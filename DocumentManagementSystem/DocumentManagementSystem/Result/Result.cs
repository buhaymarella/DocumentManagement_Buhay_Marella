namespace DocumentManagementSystem
{
    public class Result
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; }
        public object Response { get; set; }

        public Result Success(string message = null, object response = null)
        {
            IsSuccess = true;
            Message = message;
            Response = response;
            return this;
        }

        public Result Fail(string message)
        {
            IsSuccess = false;
            Message = message;
            return this;
        }
    }
}
