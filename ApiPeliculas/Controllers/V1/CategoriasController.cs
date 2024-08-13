using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers.V1
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
    [ApiVersion("1.0")] //Nos sirve para especificar la versión de la API a nivel de controlador en la cual ese controlador va ha existir o trabajar.

    /*[ApiVersion("2.0",Deprecated = true)] Cuando agregamos la propiedad Deprecated en true nos sirve para decir que versión de un controlador
     * de la Api está obsoleta y próxima a ser eliminada.
     * También podemos hacerlo agrando la siguiente declaración pero esto para un método en concreto:
     * [Obsolete("Esta endpoint está obsoleto")] y en parentesis y entre comilla ponemos un mensaje
     */
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
        //[MapToApiVersion("1.0")] //Nos sirve para especificar la versión de la API a nivel de método en la cual ese método va ha existir o trabajar.
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
        [AllowAnonymous]
        //Es una buena práctica declarar de manera explicita el parámetro que recibe el método
        //y como se va llamar dicho parámetro
        [HttpGet("{categoriaId:int}", Name = "GetCategoria")]
        [ResponseCache(CacheProfileName = "CachePorDefecto")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCategoria(int categoriaId)
        {
            var itemCategoria = _ctRepo.GetCategoria(categoriaId);

            if (itemCategoria == null)
            {
                return NotFound();
            }
            var itemCategoriaDto = _mapper.Map<CategoriaDto>(itemCategoria);

            return Ok(itemCategoriaDto);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult CrearCategoria([FromBody] CrearCategoriaDto crearCategoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (crearCategoriaDto == null)
            {
                return BadRequest(ModelState);
            }

            //Si el nombre de la categoria existe.
            if (_ctRepo.ExisteCategoria(crearCategoriaDto.Nombre))
            {
                ModelState.AddModelError("", "La categoría ya existe");
                return StatusCode(404, ModelState);
            }

            var categoria = _mapper.Map<Categoria>(crearCategoriaDto);

            if (!_ctRepo.CrearCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salio mal guardando el registro {categoria.Nombre}");
                return StatusCode(404, ModelState);
            }
            //Esta línea de código nos sirve para a la hora de la creación del registro poder visualizar sus datos como retorno
            return CreatedAtRoute("GetCategoria", new { categoriaId = categoria.Id }, categoria);
        }

        [HttpPatch("{categoriaId:int}", Name = "ActualizarPatchCategoria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarPatchCategoria(int categoriaId, [FromBody] CategoriaDto categoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (categoriaDto == null || categoriaId != categoriaDto.Id)
            {
                return BadRequest(ModelState);
            }

            var categoriaExistente = _ctRepo.GetCategoria(categoriaId);

            if (categoriaExistente == null)
            {
                return NotFound($"No se encontro la categoría con ID {categoriaId}");
            }

            var categoria = _mapper.Map<Categoria>(categoriaDto);

            if (!_ctRepo.ActualizarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salio mal actualizando el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpPut("{categoriaId:int}", Name = "ActualizarPutCategoria")] //El método put nos permite actualizar solo los campos enviados y es el más recomendable.
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarPutCategoria(int categoriaId, [FromBody] CategoriaDto categoriaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (categoriaDto == null || categoriaId != categoriaDto.Id)
            {
                return BadRequest(ModelState);
            }

            var categoriaExistente = _ctRepo.GetCategoria(categoriaId);

            if (categoriaExistente == null)
            {
                return NotFound($"No se encontro la categoría con ID {categoriaId}");
            }

            var categoria = _mapper.Map<Categoria>(categoriaDto);

            if (!_ctRepo.ActualizarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salio mal actualizando el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [HttpDelete("{categoriaId:int}", Name = "BorrarCategoria")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BorrarCategoria(int categoriaId)
        {
            if (!_ctRepo.ExisteCategoria(categoriaId))
            {
                return NotFound();

            }
            var categoria = _ctRepo.GetCategoria(categoriaId);

            if (!_ctRepo.BorrarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Algo salio mal borrando el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }
    }
}
