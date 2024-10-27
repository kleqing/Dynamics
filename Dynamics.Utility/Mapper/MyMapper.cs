using AutoMapper;
using Dynamics.Models.Dto;
using Dynamics.Models.Models;
using Dynamics.Models.Models.ViewModel;

namespace Dynamics.Utility.Mapper;

public class MyMapper : Profile
{
    public MyMapper()
    {
        CreateMap<Request, RequestOverviewDto>()
            .ForMember(
                rod => rod.Username,
                opt => opt.MapFrom(r => r.User.UserName))
            .ReverseMap();
        CreateMap<Project, ProjectOverviewDto>().ReverseMap();
        CreateMap<Organization, OrganizationOverviewDto>().ReverseMap();
        CreateMap<Project, UpdateProjectProfileRequestDto>().ReverseMap();
        CreateMap<VnPayRequestDto, UserToProjectTransactionHistory>()
            .ForMember(dest => dest.Time, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.Time, opt => opt.Ignore());

        CreateMap<VnPayRequestDto, UserToOrganizationTransactionHistory>()
            .ForMember(dest => dest.Time, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.Time, opt => opt.Ignore());

        CreateMap<VnPayRequestDto, OrganizationToProjectHistory>()
            .ForMember(dest => dest.Time, opt => opt.Ignore())
            .ReverseMap()
            .ForMember(dest => dest.Time, opt => opt.Ignore());

        CreateMap<User, UserVM>().ReverseMap();
    }
}