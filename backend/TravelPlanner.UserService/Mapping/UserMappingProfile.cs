using AutoMapper;
using TravelPlanner.Common.DTOs;
using TravelPlanner.UserService.Models;

namespace TravelPlanner.UserService.Mapping;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());
    }
}
