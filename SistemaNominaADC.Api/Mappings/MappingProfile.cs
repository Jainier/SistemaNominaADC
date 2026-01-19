using AutoMapper;
using SistemaNominaADC.Entidades;
using SistemaNominaADC.Entidades.DTO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SistemaNominaADC.API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //CreateMap<Departamento, DepartamentoDTO>()
            //.ForMember(d => d.EstadoNombre,
            //    o => o.MapFrom(s => s.Estado.Nombre));

            //CreateMap<DepartamentoDTO, Departamento>();
            CreateMap<Departamento, DepartamentoDTO>().ReverseMap();

            //CreateMap<Estado, EstadoDTO>().ReverseMap();
            //CreateMap<Puesto, PuestoDTO>().ReverseMap();
            //CreateMap<Empleado, EmpleadoDTO>().ReverseMap();
            //CreateMap<Bitacora,BitacoraDTO>().ReverseMap();
            //CreateMap<GrupoEstado, GrupoEstadoDTO>().ReverseMap();            
        }
    }
}
