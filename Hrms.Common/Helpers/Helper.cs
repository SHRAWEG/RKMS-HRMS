using Hrms.Common.Models;
using NPOI.OpenXmlFormats.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Helpers
{
    public static class Helper
    {
        public static string GetAbbreviation(string str)
        {
            string firstLetters = "";

            string[] words = str.Split(' ');

            foreach (string word in words)
            {
                firstLetters += word[0];
            }

            return firstLetters;
        }

        public static string FullName(string firstName, string middleName, string lastName)
        {
            return firstName + " " + (String.IsNullOrEmpty(middleName) ? null : middleName + " ") + lastName;
        }

        public static TimeOnly? GetEarliestTime(params TimeOnly?[] times)
        {
            TimeOnly? earliest = times.Where(t => t.HasValue).Min();
            return earliest;
        }

        public static TimeOnly? GetLatestTime(params TimeOnly?[] times)
        {
            TimeOnly? latest = times.Where(t => t.HasValue).Max();
            return latest;
        }

        public static async Task<List<DateOnly>> GetWeekends(DateOnly startDate, DateOnly endDate, int? empId, DataContext context)
        {
            List<WeekendDetail> empWeekendDetails = await context.WeekendDetails.Where(x => x.ValidFrom <= endDate && x.EmpId == empId).ToListAsync();
            List<WeekendDetail> weekendDetails = await context.WeekendDetails.Where(x => x.ValidFrom <= endDate && x.EmpId == null).ToListAsync();
            List<DateOnly> weekends = new();

            if (empWeekendDetails.Count == 0 && weekendDetails.Count == 0)
            {
                return weekends;
            }

            DateOnly date = startDate;

            do
            {
                var weekendDetail = empWeekendDetails.Where(x => x.ValidFrom <= date).OrderByDescending(x => x.ValidFrom).FirstOrDefault();

                if (weekendDetail is null)
                {
                    weekendDetail = weekendDetails.Where(x => x.ValidFrom <= date).OrderByDescending(x => x.ValidFrom).FirstOrDefault();
                    
                    if (weekendDetail is null)
                    {
                        date = date.AddDays(1);

                        continue;
                    }
                }

                if (weekendDetail.Sunday && (int)date.DayOfWeek == 0)
                {
                    weekends.Add(date);
                }

                if (weekendDetail.Monday && (int)date.DayOfWeek == 1)
                {
                    weekends.Add(date);
                }

                if (weekendDetail.Tuesday && (int)date.DayOfWeek == 2)
                {
                    weekends.Add(date);
                }

                if (weekendDetail.Wednesday && (int)date.DayOfWeek == 3)
                {
                    weekends.Add(date);
                }

                if (weekendDetail.Thursday && (int)date.DayOfWeek == 4)
                {
                    weekends.Add(date);
                }

                if (weekendDetail.Friday && (int)date.DayOfWeek == 5)
                {
                    weekends.Add(date);
                }

                if (weekendDetail.Saturday && (int)date.DayOfWeek == 6)
                {
                    weekends.Add(date);
                }

                date = date.AddDays(1);
                continue;
            } while (date <= endDate);

            return weekends;
        }

        public static string GetMonthInBs(int month)
        {
            switch (month)
            {
                case 1:
                    return "Baisakh";

                case 2:
                    return "Jestha";

                case 3:
                    return "Ashadha";

                case 4:
                    return "Shrawan";

                case 5:
                    return "Bhadra";

                case 6:
                    return "Ashwin";

                case 7:
                    return "Kartik";

                case 8:
                    return "Mangsir";

                case 9:
                    return "Paush";

                case 10:
                    return "Magh";

                case 11:
                    return "Falgun";

                case 12:
                    return "Chaitra";

                default:
                    return "Invalid";
            }
        }
    }
}
