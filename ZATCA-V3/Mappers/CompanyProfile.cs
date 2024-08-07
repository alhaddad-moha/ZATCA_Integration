using ZATCA_V3.DTOs;
using ZATCA_V3.Models;

namespace ZATCA_V3.Mappers;

using AutoMapper;

public class CompanyProfile : Profile
{
    public CompanyProfile()
    {
        CreateMap<Company, CompanyDto>();
    }
}