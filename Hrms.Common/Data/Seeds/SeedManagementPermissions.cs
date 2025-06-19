using NPOI.OpenXmlFormats.Vml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Data.Seeds
{
    public class SeedManagementPermissions
    {
        public static async Task SeedManagement(DataContext context)
        {
            List<string> categories = new()
            {
                "Attendance Management",
                "Leave Management",
                "Roster Management",
                "Holiday Management"
            };

            var permissions = await context.Permissions.Where(x => categories.Contains(x.Category)).ToListAsync();

            var managementPermissions = new List<Permission>()
            {

                #region Attendance

                    #region Regularization

                new Permission
                {

                    Category = "Attendance Management",
                    SubCategory = "Regularizations",
                    Name = "regularization-application",
                    DisplayName = "Regularization Application"
                },

                new Permission
                {

                    Category = "Attendance Management",
                    SubCategory = "Regularizations",
                    Name = "regularization-history",
                    DisplayName = "Regularization History"
                },

                new Permission
                {

                    Category = "Attendance Management",
                    SubCategory = "Regularizations",
                    Name = "regularization-cancellation",
                    DisplayName = "Regularization Cancellation"
                },

                #endregion

                    #region Regularization Type
                new Permission
                {
                    Category = "Attendance Management",
                    SubCategory = "Regularization Types",
                    Name = "list-regularization-type",
                    DisplayName = "List"
                },

                new Permission
                {
                    Category = "Attendance Management",
                    SubCategory = "Regularization Types",
                    Name = "search-regularization-type",
                    DisplayName = "Search"
                },

                new Permission
                {
                    Category = "Attendance Management",
                    SubCategory = "Regularization Types",
                    Name = "update-regularization-type",
                    DisplayName = "Update"
                },
                    #endregion

                #region Report

                new Permission
                {

                    Category = "Attendance Management",
                    SubCategory = "Reports",
                    Name = "attendance-report",
                    DisplayName = "Attendance Report"
                },

                new Permission
                {

                    Category = "Attendance Management",
                    SubCategory = "Reports",
                    Name = "attendance-calendar",
                    DisplayName = "Attendance Calendar"
                },

                new Permission
                {

                    Category = "Attendance Management",
                    SubCategory = "Reports",
                    Name = "attendance-daily-report",
                    DisplayName = "Attendance Daily Report"
                },

                new Permission
                {
                    Category = "Attendance Management",
                    SubCategory = "",
                    Name = "import-attendnace",
                    DisplayName = "Import Attendance"
                },

                    #endregion

                #endregion

                #region Leave

                new Permission
                {
                    Category = "Leave Management",
                    SubCategory = "",
                    Name = "leave-history",
                    DisplayName = "Leave History"
                },

                new Permission
                {
                    Category = "Leave Management",
                    SubCategory = "",
                    Name = "leave-application",
                    DisplayName = "Leave Application"
                },

                new Permission
                {
                    Category = "Leave Management",
                    SubCategory = "",
                    Name = "leave-cancellation",
                    DisplayName = "Leave Cancellation"
                },

                new Permission
                {
                    Category = "Leave Management",
                    SubCategory = "",
                    Name = "import-leave-balance",
                    DisplayName = "Import Leave Balance"
                },

                #endregion

                #region Roster

                new Permission
                {

                    Category = "Roster Management",
                    SubCategory = "",
                    Name = "set-roster",
                    DisplayName = "Set Roster"
                },

                new Permission
                {

                    Category = "Roster Management",
                    SubCategory = "",
                    Name = "check-roster",
                    DisplayName = "Check Roster"
                },

                #endregion

                #region Holiday

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Holidays",
                    Name = "list-holiday",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Holidays",
                    Name = "view-holiday",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Holidays",
                    Name = "search-holiday",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Holidays",
                    Name = "write-holiday",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Holidays",
                    Name = "update-holiday",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Holidays",
                    Name = "delete-holiday",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Calendars",
                    Name = "list-calendar",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Calendars",
                    Name = "view-calendar",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Calendars",
                    Name = "search-calendar",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Calendars",
                    Name = "write-calendar",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Calendars",
                    Name = "update-calendar",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Calendars",
                    Name = "delete-calendar",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Holiday Calendars",
                    Name = "list-holiday-calendar",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Holiday Calendars",
                    Name = "add-holiday-to-calendar",
                    DisplayName = "Add Holiday To Calendar"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Holiday Calendars",
                    Name = "remove-holiday-from-calendar",
                    DisplayName = "Remove Holiday From Calendar"
                },

                new Permission
                {

                    Category = "Holiday Management",
                    SubCategory = "Holiday Calendars",
                    Name = "delete-holiday-calendar",
                    DisplayName = "Delete"
                },

                #endregion
            };

            if (permissions.Any())
            {
                foreach (var permission in managementPermissions)
                {
                    if (!permissions.Any(x => x.Name == permission.Name))
                    {
                        context.Add(permission);
                    }
                }
            } else
            {
                context.AddRange(managementPermissions);
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedRecruitmentManagement(DataContext context)
        {
            var permissions = await context.Permissions.Where(x => x.Category == "Recruitment Management").ToListAsync();

            var managementPermissions = new List<Permission>()
            {
                #region Recruitment

                    #region Manpower
                
                new Permission
                {
                    Category = "Recruitment Management",
                    SubCategory = "Manpower Requisitions",
                    Name = "list-manpower-requisition",
                    DisplayName = "List"
                },

                new Permission
                {
                    Category = "Recruitment Management",
                    SubCategory = "Manpower Requisitions",
                    Name = "view-manpower-requisition",
                    DisplayName = "View"
                },

                    #endregion

                    #region Jobs

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Jobs",
                    Name = "list-job",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Jobs",
                    Name = "view-job",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Jobs",
                    Name = "search-job",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Jobs",
                    Name = "write-job",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Jobs",
                    Name = "update-job",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Jobs",
                    Name = "delete-job",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Jobs",
                    Name = "update-job-status",
                    DisplayName = "Update Status"
                },
                    #endregion

                    #region Candidates

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "list-candidate",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "view-candidate",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "search-candidate",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "write-candidate",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "update-candidate",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "delete-candidate",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "candidate-stage-history",
                    DisplayName = "Candidate Stage History"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "assign-candidate-job",
                    DisplayName = "Assign Job"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "add-candidate-source",
                    DisplayName = "Add Source"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "add-candidate-skill",
                    DisplayName = "Add Skill"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "remove-candidate-job",
                    DisplayName = "Remove Job"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "remove-candidate-source",
                    DisplayName = "Remove Source"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "remove-candidate-skill",
                    DisplayName = "Remove Skill"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "evaluate-candidate",
                    DisplayName = "Evaluate"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Candidates",
                    Name = "update-candidate-stage",
                    DisplayName = "Update Stage"
                },
                #endregion

                    #region EmploymentTypes
                    
                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Employment Types",
                    Name = "list-employment-type",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Employment Types",
                    Name = "view-employment-type",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Employment Types",
                    Name = "search-employment-type",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Employment Types",
                    Name = "write-employment-type",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Employment Types",
                    Name = "update-employment-type",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Employment Types",
                    Name = "delete-employment-type",
                    DisplayName = "Delete"
                },

	                #endregion

                    #region Sources
                    
                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Sources",
                    Name = "list-source",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Sources",
                    Name = "view-source",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Sources",
                    Name = "search-source",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Sources",
                    Name = "write-source",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Sources",
                    Name = "update-source",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Sources",
                    Name = "delete-source",
                    DisplayName = "Delete"
                },

	                #endregion

                    #region Stages
                    
                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Stages",
                    Name = "list-stage",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Stages",
                    Name = "view-stage",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Stages",
                    Name = "search-stage",
                    DisplayName = "Search"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Stages",
                    Name = "write-stage",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Stages",
                    Name = "update-stage",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Stages",
                    Name = "delete-stage",
                    DisplayName = "Delete"
                },

                new Permission
                {

                    Category = "Recruitment Management",
                    SubCategory = "Stages",
                    Name = "update-stage-step",
                    DisplayName = "Update Step"
                },

	                #endregion

                #endregion
            };

            if (permissions.Any())
            {
                foreach (var permission in managementPermissions)
                {
                    if (!permissions.Any(x => x.Name == permission.Name))
                    {
                        context.Add(permission);
                    }
                }
            }
            else
            {
                context.AddRange(managementPermissions);
            }

            await context.SaveChangesAsync();
        }

        public static async Task SeedUserManagement(DataContext context)
        {
            var permissions = await context.Permissions.Where(x => x.Category == "User Management").ToListAsync();

            var managementPermissions = new List<Permission>()
            {
                #region User Management

                    #region Roles
                    
                new Permission
                {

                    Category = "User Management",
                    SubCategory = "Roles",
                    Name = "list-role",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "User Management",
                    SubCategory = "Roles",
                    Name = "view-role",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "User Management",
                    SubCategory = "Roles",
                    Name = "write-role",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "User Management",
                    SubCategory = "Roles",
                    Name = "update-role",
                    DisplayName = "Update"
                },

                new Permission
                {

                    Category = "User Management",
                    SubCategory = "Roles",
                    Name = "delete-role",
                    DisplayName = "Delete"
                },

	                #endregion

                    #region Users

                new Permission
                {

                    Category = "User Management",
                    SubCategory = "Users",
                    Name = "list-user",
                    DisplayName = "List"
                },

                new Permission
                {

                    Category = "User Management",
                    SubCategory = "Users",
                    Name = "view-user",
                    DisplayName = "View"
                },

                new Permission
                {

                    Category = "User Management",
                    SubCategory = "Users",
                    Name = "write-user",
                    DisplayName = "Write"
                },

                new Permission
                {

                    Category = "User Management",
                    SubCategory = "Users",
                    Name = "update-user",
                    DisplayName = "Update"
                },
                #endregion

                    #region Passwords
                new Permission
                {

                    Category = "User Management",
                    SubCategory = "Passwords",
                    Name = "reset-password",
                    DisplayName = "Reset Password"
                },
                    #endregion

                #endregion
            };

            foreach (var permission in managementPermissions)
            {
                if (!permissions.Any(x => x.Name == permission.Name))
                {
                    context.Add(permission);
                }
            }

            await context.SaveChangesAsync();
        }

        //public static async Task SeedSalarySlipDocument(DataContext context)
        //{
        //    var permissions = await context.Permissions.Where(x => x.Category == "User Management").ToListAsync();

        //    var managementPermissions = new List<Permission>()
        //    {
        //        #region User Management

        //            #region Roles
                    
        //        new Permission
        //        {

        //            Category = "User Management",
        //            SubCategory = "Roles",
        //            Name = "list-role",
        //            DisplayName = "List"
        //        },

        //        new Permission
        //        {

        //            Category = "User Management",
        //            SubCategory = "Roles",
        //            Name = "view-role",
        //            DisplayName = "View"
        //        },

        //        new Permission
        //        {

        //            Category = "User Management",
        //            SubCategory = "Roles",
        //            Name = "write-role",
        //            DisplayName = "Write"
        //        },

        //        new Permission
        //        {

        //            Category = "User Management",
        //            SubCategory = "Roles",
        //            Name = "update-role",
        //            DisplayName = "Update"
        //        },

        //        new Permission
        //        {

        //            Category = "User Management",
        //            SubCategory = "Roles",
        //            Name = "delete-role",
        //            DisplayName = "Delete"
        //        },

	       //         #endregion

        //            #region Users

        //        new Permission
        //        {

        //            Category = "User Management",
        //            SubCategory = "Users",
        //            Name = "list-user",
        //            DisplayName = "List"
        //        },

        //        new Permission
        //        {

        //            Category = "User Management",
        //            SubCategory = "Users",
        //            Name = "view-user",
        //            DisplayName = "View"
        //        },

        //        new Permission
        //        {

        //            Category = "User Management",
        //            SubCategory = "Users",
        //            Name = "write-user",
        //            DisplayName = "Write"
        //        },

        //        new Permission
        //        {

        //            Category = "User Management",
        //            SubCategory = "Users",
        //            Name = "update-user",
        //            DisplayName = "Update"
        //        },
        //        #endregion

        //            #region Passwords
        //        new Permission
        //        {

        //            Category = "User Management",
        //            SubCategory = "Passwords",
        //            Name = "reset-password",
        //            DisplayName = "Reset Password"
        //        },
        //            #endregion

        //        #endregion
        //    };

        //    foreach (var permission in managementPermissions)
        //    {
        //        if (!permissions.Any(x => x.Name == permission.Name))
        //        {
        //            context.Add(permission);
        //        }
        //    }

        //    await context.SaveChangesAsync();
        ////}

    }
}
