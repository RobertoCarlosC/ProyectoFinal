namespace EnerGym.Infrastructure
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }

        public static ApiResponse<T> Ok(T data, string? message = null) =>
            new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Fail(string error, string? message = null) =>
            new() { Success = false, Error = error, Message = message };
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }

        public static ApiResponse Ok(string? message = null) =>
            new() { Success = true, Message = message };

        public static ApiResponse Fail(string error, string? message = null) =>
            new() { Success = false, Error = error, Message = message };
    }
}
