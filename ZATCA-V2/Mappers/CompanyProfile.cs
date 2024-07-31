using ZATCA_V2.DTOs;
using ZATCA_V2.Models;

namespace ZATCA_V2.Mappers;

using AutoMapper;

public class CompanyProfile : Profile
{
    public CompanyProfile()
    {
        CreateMap<Company, CompanyDto>();
    }
}