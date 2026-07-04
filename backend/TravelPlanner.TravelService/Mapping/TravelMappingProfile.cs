using AutoMapper;
using TravelPlanner.Common.DTOs;
using TravelPlanner.TravelService.Models;

namespace TravelPlanner.TravelService.Mapping;

public class TravelMappingProfile : Profile
{
    public TravelMappingProfile()
    {
        CreateMap<TravelPlan, TravelPlanDto>();
        CreateMap<CreateTravelPlanDto, TravelPlan>();

        CreateMap<Destination, DestinationDto>();
        CreateMap<CreateDestinationDto, Destination>();

        CreateMap<Activity, ActivityDto>();
        CreateMap<CreateActivityDto, Activity>();

        CreateMap<ChecklistItem, ChecklistItemDto>();
        CreateMap<CreateChecklistItemDto, ChecklistItem>();
    }
}
