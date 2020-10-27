using Microsoft.AspNetCore.Mvc;
using SkillsGardenDTO.Error;

namespace SkillsGardenApi.Utils
{
    public static class ForbiddenObjectResult
    {
        public static ObjectResult Create(ErrorResponse errorResponse)
        {
            var objectResult = new ObjectResult(errorResponse);
            objectResult.StatusCode = 403;
            return objectResult;
        }
    }
}
