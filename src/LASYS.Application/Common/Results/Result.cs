namespace LASYS.Application.Common.Results
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public string ErrorOrDefault => string.IsNullOrWhiteSpace(Error) 
            ? "An unexpected error occurred." : Error;
        protected Result(bool isSuccess, string? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }
        public static Result Success()
            => new Result(true, null);
        public static Result Failure(string error)
            => new Result(false, error);
        public static Result<T> Success<T>(T value)
            => new Result<T>(value, true, null);
        public static Result<T> Failure<T>(string error)
            => new Result<T>(default, false, error);
    }
    public class Result<T> : Result
    {
        public T? Value { get; }
        internal Result(T? value, bool isSuccess, string? error)
            : base(isSuccess, error)
        {
            Value = value;
        }
    }
}
