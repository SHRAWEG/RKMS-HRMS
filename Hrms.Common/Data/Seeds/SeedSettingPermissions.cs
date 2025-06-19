using Hrms.Common.Data.Migrations;
using NPOI.OpenXmlFormats.Vml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hrms.Common.Data.Seeds
{
    public class SeedSettingPermissions
    {
        public static async Task SeedSettingPermission(DataContext context)
        {
            var permissions = await context.Permissions.Where(x => x.Category == "Settings").ToListAsync();

            var settingPermissions = new List<Permission>()
            {
                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Banks",
                    Name = "list-bank",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Banks",
                    Name = "view-bank",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Banks",
                    Name = "search-bank",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Banks",
                    Name = "write-bank",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Banks",
                    Name = "update-bank",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Banks",
                    Name = "delete-bank",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Blood Groups",
                    Name = "list-blood-group",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Blood Groups",
                    Name = "view-blood-group",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Blood Groups",
                    Name = "search-blood-group",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Blood Groups",
                    Name = "write-blood-group",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Blood Groups",
                    Name = "update-blood-group",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Blood Groups",
                    Name = "delete-blood-group",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cities",
                    Name = "list-city",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cities",
                    Name = "view-city",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cities",
                    Name = "search-city",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cities",
                    Name = "write-city",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cities",
                    Name = "update-city",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cities",
                    Name = "delete-city",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cost Centers",
                    Name = "list-cost-center",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cost Centers",
                    Name = "view-cost-center",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cost Centers",
                    Name = "search-cost-center",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cost Centers",
                    Name = "write-cost-center",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cost Centers",
                    Name = "update-cost-center",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Cost Centers",
                    Name = "delete-cost-center",
                    DisplayName = "Delete"
                },
                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Countries",
                    Name = "list-country",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Countries",
                    Name = "view-country",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Countries",
                    Name = "search-country",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Countries",
                    Name = "write-country",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Countries",
                    Name = "update-country",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Countries",
                    Name = "delete-country",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Divisions",
                    Name = "list-division",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Divisions",
                    Name = "view-division",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Divisions",
                    Name = "search-division",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Divisions",
                    Name = "write-division",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Divisions",
                    Name = "update-division",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Divisions",
                    Name = "delete-division",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Education Levels",
                    Name = "list-education-level",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Education Levels",
                    Name = "view-education-level",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Education Levels",
                    Name = "search-education-level",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Education Levels",
                    Name = "write-education-level",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Education Levels",
                    Name = "update-education-level",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Education Levels",
                    Name = "delete-education-level",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Emp Categories",
                    Name = "list-emp-category",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Emp Categories",
                    Name = "view-emp-category",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Emp Categories",
                    Name = "search-emp-category",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Emp Categories",
                    Name = "write-emp-category",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Emp Categories",
                    Name = "update-emp-category",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Emp Categories",
                    Name = "delete-emp-category",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Gate Pass Types",
                    Name = "list-gate-pass-type",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Gate Pass Types",
                    Name = "view-gate-pass-type",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Gate Pass Types",
                    Name = "search-gate-pass-type",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Gate Pass Types",
                    Name = "write-gate-pass-type",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Gate Pass Types",
                    Name = "update-gate-pass-type",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Gate Pass Types",
                    Name = "delete-gate-pass-type",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Plants",
                    Name = "list-plant",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Plants",
                    Name = "view-plant",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Plants",
                    Name = "search-plant",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Plants",
                    Name = "write-plant",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Plants",
                    Name = "update-plant",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Plants",
                    Name = "delete-plant",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Regions",
                    Name = "list-region",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Regions",
                    Name = "view-region",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Regions",
                    Name = "search-region",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Regions",
                    Name = "write-region",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Regions",
                    Name = "update-region",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Regions",
                    Name = "delete-region",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Religions",
                    Name = "list-religion",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Religions",
                    Name = "view-religion",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Religions",
                    Name = "search-religion",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Religions",
                    Name = "write-religion",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Religions",
                    Name = "update-religion",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Religions",
                    Name = "delete-religion",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "States",
                    Name = "list-state",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "States",
                    Name = "view-state",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "States",
                    Name = "search-state",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "States",
                    Name = "write-state",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "States",
                    Name = "update-state",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "States",
                    Name = "delete-state",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Statuses",
                    Name = "list-status",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Statuses",
                    Name = "view-status",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Statuses",
                    Name = "search-status",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Titles",
                    Name = "list-title",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Titles",
                    Name = "view-title",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Titles",
                    Name = "search-title",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Titles",
                    Name = "write-title",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Titles",
                    Name = "update-title",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Titles",
                    Name = "delete-title",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Uniform Types",
                    Name = "list-uniform-type",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Uniform Types",
                    Name = "view-uniform-type",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Uniform Types",
                    Name = "search-uniform-type",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Uniform Types",
                    Name = "write-uniform-type",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Uniform Types",
                    Name = "update-uniform-type",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Settings",
                    SubCategory = "Uniform Types",
                    Name = "delete-uniform-type",
                    DisplayName = "Delete"
                }
            };

            if (permissions.Any())
            {
                foreach (var permission in settingPermissions)
                {
                    if (!permissions.Any(x => x.Name == permission.Name))
                    {
                        context.Add(permission);
                    }
                }
            }
            else
            {
                context.AddRange(settingPermissions);
            }

            await context.SaveChangesAsync();
        }
    }
}
