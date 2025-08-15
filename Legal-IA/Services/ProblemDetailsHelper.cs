using Microsoft.AspNetCore.Mvc;

namespace Legal_IA.Services;

public static class ProblemDetailsHelper
{
    public static IActionResult ValidationProblem(IEnumerable<string> errors, string title = "Validation Failed")
    {
        var problemDetails = new ValidationProblemDetails
        {
            Title = title,
            Status = 400,
            Detail = "See the errors property for details."
        };
        problemDetails.Errors.Add("ValidationError", errors.ToArray());
        return new BadRequestObjectResult(problemDetails);
    }
}
