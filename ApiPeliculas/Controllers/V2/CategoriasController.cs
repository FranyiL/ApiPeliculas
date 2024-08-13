using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.V2
{
    /*
     * Forma de implementar la cache a nivel del controlador.
     * [ResponseCache(Duration = 20)]
     *La propiedad Duration sierve para pasarle
     *el número de segundos que va durar la primera respuesta del método en cache.
      
      Forma para utilizar un perfil global de cache
      [ResponseCache(CacheProfileName = "CachePorDefecto")]
     */
    //Dar protección al controlador completo
    [Authorize(Roles = "admin")]
    [Route("api/v{version:apiVersion}/categorias")]
    [ApiController]
    /*Con EnableCors("PoliticaCors") lo usuamos para aplicarle la politica de los CORS a un controlador de la API.
         En este caso al controlador solo se podrá acceder desde un dominio que cumpla con las políticas de los CORS de la API
         */
    //[EnableCors("PoliticaCors")]    
    [ApiVersion("2.0")] //Nos sirve para especificar la versión de la API a nivel de controlador en la cual ese controlador va ha existir o trabajar.
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepositorio _ctRepo;
        private readonly IMapper _mapper;

        public CategoriasController(ICategoriaRepositorio ctRepo, IMapper mapper)
        {
            _ctRepo = ctRepo;
            _mapper = mapper;
        }

        [AllowAnonymous] //Nos sirva para exponer un método de forma pública a pesar de tener una protección en el controlador
        [HttpGet]
        /*[MapToApiVersion("1.0")]*/ //Nos sirve para especificar la versión de la API a nivel de método en la cual ese método va ha existir o trabajar.
        [ResponseCache(CacheProfileName = "CachePorDefecto")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        /*Con EnableCors("PoliticaCors") lo usuamos para aplicarle la politica de los CORS a un método de la API esto
         por lo regular cuando el método no esta en el program.cs de manera global.
         En este caso el método solo se podrá consumir desde un dominio que cumpla con las políticas de los CORS de la API
         */
        //[EnableCors("PoliticaCors")]
        public IActionResult GetCategorias()
        {
            var listaCategorias = _ctRepo.GetCategorias();
            var listaCategoriasDto = new List<CategoriaDto>();

            foreach (var lista in listaCategorias)
            {
                listaCategoriasDto.Add(_mapper.Map<CategoriaDto>(lista));
            }

            return Ok(listaCategoriasDto);
        }
    }
}
