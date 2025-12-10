using AutoMapper;
using BackendAPI.Application.DTOs.Repository;
using BackendAPI.Domain.Entities;

namespace BackendAPI.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Repository, RepositoryDto>();
            CreateMap<RepositoryDto, Repository>();
        }
    }
}