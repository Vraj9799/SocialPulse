using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using SocialMediaApp.Models.Shared;

namespace SocialMediaApp.Filters
{
    public class ModelValidationFilter(ILogger<ModelValidationFilter> logger) : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(m => m.Value.Errors.Any())
                    .ToDictionary(k => k.Key,
                        k => k.Value.Errors.Select(e => e.ErrorMessage).ToList());
                logger.LogError("Model State Validation Errors --> {Error}", errors);
                context.Result = new BadRequestObjectResult(
                    new ApiResponse<Dictionary<string, List<string>>>(HttpStatusCode.BadRequest,errors,false));
            }
            base.OnActionExecuting(context);
        }
    }
}
