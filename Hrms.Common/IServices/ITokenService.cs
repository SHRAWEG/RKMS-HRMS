namespace Hrms.Common.Interfaces
{
    public interface ITokenService
    {
        Task<string> CreateToken(User user);
    }
}
