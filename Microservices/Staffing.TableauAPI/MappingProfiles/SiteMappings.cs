using AutoMapper;
using Staffing.TableauAPI.Dtos;
using Staffing.TableauAPI.Entities;

namespace Staffing.TableauAPI.MappingProfiles
{
    public class SiteMappings : Profile
    {
        public SiteMappings()
        {
            CreateMap<SiteEntity, SiteDto>().ReverseMap();
            CreateMap<SiteEntity, SiteUpdateDto>().ReverseMap();
            CreateMap<SiteEntity, SiteCreateDto>().ReverseMap();
        }
    }
}
