using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MiniSiniestros.Dto;
using MiniSiniestros.Entities;

namespace MiniSiniestros.Services.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Siniestro, SiniestroDto>();
        CreateMap<Prestador, PrestadorDto>();
    }
}