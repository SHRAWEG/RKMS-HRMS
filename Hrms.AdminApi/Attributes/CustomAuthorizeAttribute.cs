using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Hrms.AdminApi.Attributes;

public class CustomAuthorizeAttribute : TypeFilterAttribute
{
    public CustomAuthorizeAttribute(params string[] permissions) : base(typeof(CustomAuthorizeFilter))
    {
        Arguments = new object[] { permissions };
    }
}

public class CustomAuthorizeFilter : IAsyncAuthorizationFilter
{
    private readonly string[] _permissions;
    private readonly DataContext _context;

    public CustomAuthorizeFilter(string[] permissions, DataContext context)
    {
        _permissions = permissions ?? throw new ArgumentNullException(nameof(permissions));
        _context = context;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userId = user.GetUserId();
        if (userId == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (user.GetUserRole() == "super-admin" || user.GetUserRole() == "admin") return;

        var userPermissions = await _context.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission.Name)
            .ToListAsync();

        foreach (var permission in _permissions)
        {
            if (userPermissions.Contains(permission))
            {
                return;
            } else
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}