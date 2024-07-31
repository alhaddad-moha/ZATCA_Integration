using Microsoft.AspNetCore.Mvc;

namespace ZATCA_V2.Responses;

public class ApiResponse<T> : IActionResult
{
    public int Status { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public  Dictionary<string, List<string>>? Errors { get; set; }

    public ApiResponse(int status, string message, T? data = default,  Dictionary<string, List<string>>? errors = null)
    {
        Status = status;
        Message = message;
        Data = data;
        Errors = errors;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        var objectResult = new ObjectResult(this)
        {
            StatusCode = Status
        };

        await objectResult.ExecuteResultAsync(context);
    }
}

public class ErrorDetail
{
    public string Property { get; set; }
    public string Message { get; set; }
}