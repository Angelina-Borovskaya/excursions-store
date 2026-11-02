using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerAuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Проверяем Authorize на методе
        bool hasAuthorizeOnMethod =
            context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        // Проверяем Authorize на контроллере (с защитой от null)
        bool hasAuthorizeOnClass =
            context.MethodInfo.DeclaringType != null &&
            context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

        bool hasAuthorize = hasAuthorizeOnMethod || hasAuthorizeOnClass;

        if (!hasAuthorize)
            return;

        operation.Security = new List<OpenApiSecurityRequirement>
        {
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            }
        };
    }
}
