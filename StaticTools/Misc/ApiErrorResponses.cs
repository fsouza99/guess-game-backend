using App.Applications;
using Microsoft.AspNetCore.Mvc;

namespace App.StaticTools;

public static class ApiErrorResponses
{
    public static ActionResult AppProblem(Error error)
    {
        return new ObjectResult(
            new ProblemDetails
            {
                Status = error.StatusCode,
                Title = error.Title,
                Detail = error.Description
            })
            {
                StatusCode = error.StatusCode
            };
    }
}
