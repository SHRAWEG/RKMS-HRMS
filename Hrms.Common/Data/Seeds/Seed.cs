using Hrms.Common.Enumerations;

namespace Hrms.Common.Data
{
    public class Seed
    {
        public static async Task SeedUsers(UserManager<User> userManager,
            RoleManager<Role> roleManager)
        {
            if (await userManager.Users.AnyAsync()) return;

            var roles = new List<Role>
            {
                new Role{Name = AppRole.SuperAdmin },
                new Role{Name = AppRole.Admin },
                new Role{Name = AppRole.Employee }
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            var admin = new User
            {
                UserName = "admin",
                IsActive = true,
            };

            await userManager.CreateAsync(admin, "admin");
            await userManager.AddToRolesAsync(admin, new[] { AppRole.SuperAdmin });
        }

        public static async Task SeedHiringStages(DataContext context)
        {
            if (await context.HiringStages.AnyAsync()) return;

            var data = new List<HiringStage>
            {
                new HiringStage
                {
                    Name = "Applied",
                    Step = 1,
                    IsFixed = true
                },
                new HiringStage
                {
                    Name = "Offered",
                    Step = 2,
                    IsFixed = true
                },
                new HiringStage
                {
                    Name = "Hired",
                    Step = 3,
                    IsFixed = true
                },
                new HiringStage
                {
                    Name = "Rejected",
                    Step = 4,
                    IsFixed = true
                },
            };

            context.AddRange(data);
            await context.SaveChangesAsync();
        }

        public static async Task SeedSalaryHeadMasters(DataContext context)
        {
            using (var transaction = await context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (!await context.SalaryHeadCategories.AnyAsync())
                    {
                        var shCategories = new List<SalaryHeadCategory>
                        {
                            //new SalaryHeadCategory
                            //{
                            //    Name= "Basic Salary",
                            //    ShcId= 1,
                            //    Category= "BSALARY",
                            //    Shc_Type= SalaryHeadCategoryType.Earning.Id,
                            //    SNO=1,
                            //    ShowCategory= false,
                            //    FlgUse= true,
                            //    FlgAssign= true,
                            //},
                            //new SalaryHeadCategory
                            //{
                            //    Name= "Grade",
                            //    ShcId= 2,
                            //    Category= "GRADE",
                            //    Shc_Type= SalaryHeadCategoryType.Earning.Id,
                            //    SNO=2,
                            //    ShowCategory= false,
                            //    FlgUse= true,
                            //    FlgAssign= true,
                            //},
                            new SalaryHeadCategory
                            {
                                Name= "Allowance",
                                ShcId= 3,
                                Category= "ALLOWANCE",
                                Shc_Type= SalaryHeadCategoryType.Earning.Id,
                                SNO=3,
                                ShowCategory= true,
                                FlgUse= true,
                                FlgAssign= true,
                            },
                            new SalaryHeadCategory
                            {
                                Name= "Deduction",
                                ShcId= 4,
                                Category= "DEDUCTION",
                                Shc_Type= SalaryHeadCategoryType.Deduction.Id,
                                SNO=4,
                                ShowCategory= true,
                                FlgUse= true,
                                FlgAssign= true,
                            },
                            //new SalaryHeadCategory
                            //{
                            //    Name= "Taxable Payment",
                            //    ShcId= 5,
                            //    Category= "TAXABLEPAYMENT",
                            //    Shc_Type= SalaryHeadCategoryType.Earning.Id,
                            //    SNO=5,
                            //    ShowCategory= true,
                            //    FlgUse= true,
                            //    FlgAssign= true,
                            //},
                            //new SalaryHeadCategory
                            //{
                            //    Name= "Taxable Reduction",
                            //    ShcId= 6,
                            //    Category= "TAXABLEREDUCTION",
                            //    Shc_Type= SalaryHeadCategoryType.Deduction.Id,
                            //    SNO = 6,
                            //    ShowCategory= true,
                            //    FlgUse= true,
                            //    FlgAssign= true,
                            //},
                            //new SalaryHeadCategory
                            //{
                            //    Name= "Extra Facility",
                            //    ShcId= 7,
                            //    Category= "EXTRAFACILITY",
                            //    Shc_Type= SalaryHeadCategoryType.Earning.Id,
                            //    SNO=7,
                            //    ShowCategory= false,
                            //    FlgUse= true,
                            //    FlgAssign= false,
                            //},
                            //new SalaryHeadCategory
                            //{
                            //    Name= "Overtime",
                            //    ShcId= 8,
                            //    Category= "OVERTIME",
                            //    Shc_Type= SalaryHeadCategoryType.Earning.Id,
                            //    SNO=8,
                            //    ShowCategory=false,
                            //    FlgUse= true,
                            //    FlgAssign= false,
                            //},
                            //new SalaryHeadCategory
                            //{
                            //    Name= "Extra Allowance",
                            //    ShcId= 9,
                            //    Category= "EXTRAALLOWANCE",
                            //    Shc_Type= SalaryHeadCategoryType.Earning.Id,
                            //    SNO=9,
                            //    ShowCategory= false,
                            //    FlgUse= true,
                            //    FlgAssign= false,
                            //},
                            //new SalaryHeadCategory
                            //{
                            //    Name= "Extra Day Work",
                            //    ShcId= 10,
                            //    Category= "EXTRADAYWORK",
                            //    Shc_Type= SalaryHeadCategoryType.Earning.Id,
                            //    SNO=10,
                            //    ShowCategory= false,
                            //    FlgUse= true,
                            //    FlgAssign= false,
                            //},
                            //new SalaryHeadCategory
                            //{
                            //    Name= "Absence",
                            //    ShcId= 11,
                            //    Category= "ABSENCE",
                            //    Shc_Type= SalaryHeadCategoryType.Earning.Id,
                            //    SNO=11,
                            //    ShowCategory= false,
                            //    FlgUse= true,
                            //    FlgAssign= false,
                            //},
                        };

                        context.AddRange(shCategories);
                        await context.SaveChangesAsync();
                    }

                    if (!await context.SalaryHeads.AnyAsync())
                    {
                        var salaryHeads = new List<SalaryHead>
                {
                    new SalaryHead
                    {
                        ShcId = 1,
                        Name = "Basic Salary",
                        //ShDataType = "AMOUNT",
                        //ShCalcType = "MONTHLY",
                        //IsTaxable = true,
                        //IsLocked = true,
                    },
                    //new SalaryHead
                    //{
                    //    ShcId=2,
                    //    Name = "Grade",
                        //ShDataType = "AMOUNT",
                        //ShCalcType = "MONTHLY",
                        //IsTaxable = true,
                        //IsLocked = true,
                    //},
                };

                        context.AddRange(salaryHeads);
                        await context.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

    }
}

