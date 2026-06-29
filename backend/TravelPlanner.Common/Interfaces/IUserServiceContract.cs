using TravelPlanner.Common.DTOs;

namespace TravelPlanner.Common.Interfaces;

public interface IUserServiceContract
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto dto);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> ValidateTokenAsync(string token);
}
