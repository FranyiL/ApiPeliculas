using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using AutoMapper;

namespace ApiPeliculas.PeliculasMapper
{
    public class PeliculasMapper : Profile
    {
        public PeliculasMapper()
        {
            //El método ReverseMap es usado para que haya una comunicación bidireccional entre el dto y el modelo
            CreateMap<Categoria, CategoriaDto>().ReverseMap();
            CreateMap<Categoria, CrearCategoriaDto>().ReverseMap();
            CreateMap<Pelicula, PeliculaDto>().ReverseMap();
            CreateMap<Pelicula, CrearPeliculaDto>().ReverseMap();
            CreateMap<Pelicula, ActualizarPeliculaDto>().ReverseMap();
            CreateMap<AppUsuario, UsuarioDatosDto>().ReverseMap();
            CreateMap<AppUsuario, UsuarioDto>().ReverseMap();

            //CreateMap<Usuario, UsuarioLoginDto>().ReverseMap();
            //CreateMap<Usuario, UsuarioLoginRespuestaDto>().ReverseMap();
            //CreateMap<Usuario, UsuarioRegistroDto>().ReverseMap();
        }
    }
}
