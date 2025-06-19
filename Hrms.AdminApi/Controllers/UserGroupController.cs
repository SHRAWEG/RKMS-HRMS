using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Hrms.AdminApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize(Roles = "super-admin, admin")]
    public class UserGroupController : ControllerBase
    {
        private readonly DataContext _context;

        public UserGroupController(DataContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Get(int page, int limit, string sortColumn, string sortDirection, string name)
        {
            var query = _context.UGroups.AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(b => b.Name!.ToLower().Contains(name.ToLower()));
            }

            Expression<Func<UGroup, object>> field = sortColumn switch
            {
                "Name" => x => x.Name,
                _ => x => x.Id
            };

            if (sortDirection == null)
            {
                query = query.OrderByDescending(p => p.Id);
            }
            else if (sortDirection == "asc")
            {
                query = query.OrderBy(field);
            }
            else
            {
                query = query.OrderByDescending(field);
            }

            var data = await PagedList<UGroup>.CreateAsync(query.AsNoTracking(), page, limit);

            return Ok(new
            {
                Data = data,
                data.TotalCount,
                data.TotalPages
            });
        }


        [HttpGet("All")]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.UGroups
     .Select(u => new
     {
         u.Id,
         u.Name,
         u.CreatedAt,
         u.UpdatedAt
     })
     .ToListAsync();


            return Ok(new
            {
                Data = data
            });
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _context.UGroups.Select(u => new
            {
                u.Id,
                u.Name,
                u.CreatedAt,
                u.UpdatedAt
            }).FirstOrDefaultAsync(x => x.Id == id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            return Ok(new
            {
                UGroup = data
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] string Name)
        {
            var group = new UGroup()
            {
                Name = Name
            };
            _context.UGroups.Add(group);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Group created", id = group.Id });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, string Name)
        {
            var data = await _context.UGroups.FirstOrDefaultAsync(c => c.Id == id);

            data.Name = Name;
            data.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _context.UGroups.FindAsync(id);

            if (data == null)
            {
                return ErrorHelper.ErrorResult("Id", "Id is invalid.");
            }

            if (await _context.UserGroups.AnyAsync(x => x.UGroup.Id == id))
            {
                return ErrorHelper.ErrorResult("Id", "Group is already in use.");
            }

            _context.UGroups.Remove(data);
            await _context.SaveChangesAsync();

            return Ok();
        }


        [HttpPost("{groupId}/users")]
        public async Task<IActionResult> AddUsersToGroup(int groupId, [FromBody] List<int> userIds)
        {
            var group = await _context.UGroups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            foreach (var userId in userIds)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found");
                }

                if (!_context.UserGroups.Any(ug => ug.GroupId == groupId && ug.UserId == userId))
                {
                    var userGroup = new UserGroup
                    {
                        GroupId = groupId,
                        UserId = userId
                    };
                    _context.UserGroups.Add(userGroup);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Users added to group" });
        }


        [HttpDelete("{groupId}/users")]
        public async Task<IActionResult> RemoveUsersFromGroup(int groupId, [FromBody] List<int> userIds)
        {
            var group = await _context.UGroups.FindAsync(groupId);
            if (group == null)
            {
                return NotFound();
            }

            foreach (var userId in userIds)
            {
                var userGroup = await _context.UserGroups
                    .FirstOrDefaultAsync(ug => ug.GroupId == groupId && ug.UserId == userId);
                if (userGroup == null)
                {
                    return NotFound($"User with ID {userId} not found in group");
                }

                _context.UserGroups.Remove(userGroup);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Users removed from group" });
        }


        [HttpGet("{groupId}/users")]
        public async Task<IActionResult> GetUsersInGroup(int groupId)
        {
            var users = await _context.UserGroups
                .Where(ug => ug.GroupId == groupId)
                .Select(ug => new { ug.User.Id, ug.User.UserName, ug.User.Email })
                .ToListAsync();

            return Ok(users);
        }

    }




}
