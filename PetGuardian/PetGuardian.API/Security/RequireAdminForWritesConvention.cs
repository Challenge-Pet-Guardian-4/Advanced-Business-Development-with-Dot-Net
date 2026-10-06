using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Routing;

namespace PetGuardian.API.Security;

/// <summary>Exige perfil Admin nas ações de escrita dos controllers informados (leitura continua livre p/ autenticados).</summary>
public sealed class RequireAdminForWritesConvention(params string[] controllerNames) : IControllerModelConvention
{
    private static readonly HashSet<string> MetodosSeguros = new(StringComparer.OrdinalIgnoreCase) { "GET", "HEAD", "OPTIONS" };
    private readonly HashSet<string> _controllers = new(controllerNames, StringComparer.OrdinalIgnoreCase);

    public void Apply(ControllerModel controller)
    {
        if (!_controllers.Contains(controller.ControllerName))
            return;

        foreach (var action in controller.Actions)
        {
            var metodos = action.Selectors
                .SelectMany(s => s.ActionConstraints)
                .OfType<HttpMethodActionConstraint>()
                .SelectMany(c => c.HttpMethods)
                .ToArray();

            if (metodos.Length == 0 || metodos.Any(m => !MetodosSeguros.Contains(m)))
                action.Filters.Add(new AuthorizeFilter(AuthorizationPolicies.AdminOnly));
        }
    }
}