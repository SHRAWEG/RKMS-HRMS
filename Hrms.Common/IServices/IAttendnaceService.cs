namespace Hrms.Common.Interfaces
{
    public interface IAttendnaceService
    {
        Task<double> CalculateAttendance(int empId, DateOnly fromDate, DateOnly toDate);
    }
}
