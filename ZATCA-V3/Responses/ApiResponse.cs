using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ZATCA_V3.Responses;

public class ApiResponse<T> : IActionResult
{
    public int Status { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }
    public Dictionary<string, List<string>>? Errors { get; set; }

    public ApiResponse(int status, string message, T? data = default, Dictionary<string, List<string>>? errors = null)
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

    public static ApiResponse<object> HandleErrorResponse(string errorMessage)
    {
        try
        {
            // Extract the JSON part of the error message
            int jsonStartIndex = errorMessage.IndexOf('{');
            if (jsonStartIndex != -1)
            {
                string jsonPart = errorMessage.Substring(jsonStartIndex);

                // Try to parse the JSON part
                var parsedJson = JsonConvert.DeserializeObject<JObject>(jsonPart);

                // Check if the parsed JSON contains an array of errors
                if (parsedJson.ContainsKey("errors"))
                {
                    var errors = parsedJson["errors"].ToObject<List<ErrorDetail>>();

                    // Convert errors list to dictionary format
                    var errorsDictionary = new Dictionary<string, List<string>>();
                    foreach (var error in errors)
                    {
                        if (!errorsDictionary.ContainsKey(error.Code))
                        {
                            errorsDictionary[error.Code] = new List<string>();
                        }

                        errorsDictionary[error.Code].Add(error.Message);
                    }

                    // Return the ApiResponse with the formatted error message and 400 status code
                    return new ApiResponse<object>(400, "An error occurred during CSID generation", null,
                        errorsDictionary);
                }
            }

            return new ApiResponse<object>(500, errorMessage, null);
        }
        catch (Exception ex)
        {
            return new ApiResponse<object>(500, " An error occurred during CSID generation:" + ex.Message, null);
        }
    }

    public class ErrorDetail
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}