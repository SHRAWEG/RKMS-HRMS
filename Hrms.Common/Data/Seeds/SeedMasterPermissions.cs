using NPOI.OpenXmlFormats.Vml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hrms.Common.Data.Seeds
{
    public class SeedMasterPermissions
    {
        public static async Task SeedMasterPermission(DataContext context)
        {
            var permissions = await context.Permissions.Where(x => x.Category == "Masters").ToListAsync();

            var masterPermissions = new List<Permission>()
            {
                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Asset Types",
                    Name = "list-asset-type",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Asset Types",
                    Name = "view-asset-type",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Asset Types",
                    Name = "search-asset-type",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Asset Types",
                    Name = "write-asset-type",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Asset Types",
                    Name = "update-asset-type",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Asset Types",
                    Name = "delete-asset-type",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Business Units",
                    Name = "list-business-unit",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Business Units",
                    Name = "view-business-unit",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Business Units",
                    Name = "search-business-unit",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Business Units",
                    Name = "write-business-unit",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Business Units",
                    Name = "update-business-unit",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Business Units",
                    Name = "delete-business-unit",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Companies",
                    Name = "list-company",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Companies",
                    Name = "view-company",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Companies",
                    Name = "search-company",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Companies",
                    Name = "write-company",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Companies",
                    Name = "update-company",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Companies",
                    Name = "delete-company",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Departments",
                    Name = "list-department",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Departments",
                    Name = "view-department",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Departments",
                    Name = "search-department",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Departments",
                    Name = "write-department",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Departments",
                    Name = "update-department",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Departments",
                    Name = "delete-department",
                    DisplayName = "Delete"
                },
                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Designations",
                    Name = "list-designation",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Designations",
                    Name = "view-designation",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Designations",
                    Name = "search-designation",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Designations",
                    Name = "write-designation",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Designations",
                    Name = "update-designation",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Designations",
                    Name = "delete-designation",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Devices",
                    Name = "list-device",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Devices",
                    Name = "view-device",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Devices",
                    Name = "search-device",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Devices",
                    Name = "write-device",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Devices",
                    Name = "update-device",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Devices",
                    Name = "delete-device",
                    DisplayName = "Delete"
                },

                #region Employee

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "list-employee",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "view-employee",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "search-employee",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "write-employee",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "update-employee",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "import-employee",
                    DisplayName = "Import New Emps"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "update-employee-through-import",
                    DisplayName = "Update Emps through import"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "export-employee",
                    DisplayName = "Export Employees"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "employee-transaction",
                    DisplayName = "Transaction"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "employee-leave-management",
                    DisplayName = "Employee Leave Management"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "asset-management",
                    DisplayName = "Asset Management"
                },

                new Permission
                {
                    Category="Masters",
                    SubCategory="Employees",
                    Name="view-employee-calendar",
                    DisplayName="View Calendar"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "assign-employee-calendar",
                    DisplayName = "Assign Calendar"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "register-employee",
                    DisplayName = "Register"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Employees",
                    Name = "update-employee-status",
                    DisplayName = "Update Status"
                },

                #endregion

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Grades",
                    Name = "list-grade",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Grades",
                    Name = "view-grade",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Grades",
                    Name = "search-grade",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Grades",
                    Name = "write-grade",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Grades",
                    Name = "update-grade",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Grades",
                    Name = "delete-grade",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Leaves",
                    Name = "list-leave",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Leaves",
                    Name = "view-leave",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Leaves",
                    Name = "search-leave",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Leaves",
                    Name = "write-leave",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Leaves",
                    Name = "update-leave",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Leaves",
                    Name = "delete-leave",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Locations",
                    Name = "list-location",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Locations",
                    Name = "view-location",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Locations",
                    Name = "search-location",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Locations",
                    Name = "write-location",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Locations",
                    Name = "update-location",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Locations",
                    Name = "delete-location",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Shifts",
                    Name = "list-shift",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Shifts",
                    Name = "view-shift",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Shifts",
                    Name = "search-shift",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Shifts",
                    Name = "write-shift",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Shifts",
                    Name = "update-shift",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Shifts",
                    Name = "delete-shift",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Shifts",
                    Name = "make-default-shift",
                    DisplayName = "Make Default"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Weekends",
                    Name = "list-weekend",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Weekends",
                    Name = "view-weekend",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Weekends",
                    Name = "search-weekend",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Weekends",
                    Name = "write-weekend",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Weekends",
                    Name = "update-weekend",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Masters",
                    SubCategory = "Weekends",
                    Name = "delete-weekend",
                    DisplayName = "Delete"
                }
            };

            if (permissions.Any())
            {
                foreach(var permission in masterPermissions)
                {
                    if (!permissions.Any(x => x.Name == permission.Name))
                    {
                        context.Add(permission);
                    }
                }
            } else
            {
                context.AddRange(masterPermissions);
            }

            await context.SaveChangesAsync();
        }
    }
}
