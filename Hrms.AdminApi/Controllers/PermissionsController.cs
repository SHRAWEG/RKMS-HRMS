using Hrms.Common.Data.Seeds;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Hrms.AdminApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PermissionsController : ControllerBase
{
    private readonly DataContext _context;

    public PermissionsController(DataContext context)
    {
        _context = context;
    }

    [CustomAuthorize("write-role", "update-role", "write-user", "update-user")]
    [HttpGet()]
    public async Task<IActionResult> GetAll()
    {
        var permissions = await _context.Permissions.ToListAsync();

        var data = permissions.GroupBy(x => x.Category).Select(x => new
        {
            Category = x.Key,
            SubCategories = x.GroupBy(x => x.SubCategory).Select(x => new
            {
                SubCategory = x.Key,
                Permissions = x
            })
        }).ToList();

        return Ok(new
        {
            Data = data.Select(x => new
            {
                Category = x.Category,
                SubCategories = x.SubCategories.Select(x => new
                {
                    Name = x.SubCategory,
                    Permissions = x.Permissions.Select(x => new
                    {
                        Id = x.Id,
                        Name = x.Name,
                        DisplayName = x.DisplayName
                    })
                })
            })
        });
    }

    [Authorize(Roles = "super-admin")]
    [HttpGet("Seed")]
    public async Task<IActionResult> SeedPermission()
    {
        await SeedMasterPermissions.SeedMasterPermission(_context);
        await SeedSettingPermissions.SeedSettingPermission(_context);
        await SeedManagementPermissions.SeedManagement(_context);
        await SeedManagementPermissions.SeedRecruitmentManagement(_context);
        await SeedManagementPermissions.SeedUserManagement(_context);

        return Ok();
    }
}