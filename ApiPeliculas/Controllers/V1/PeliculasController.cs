using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ApiPeliculas.Controllers.V1
{
    [Authorize(Roles = "Admin")]
    [Route("api/v{version:apiVersion}/peliculas")]
    [ApiController]
    [ApiVersion("1.0")]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaRepositorio _pelRepo;
        private readonly IMapper _mapper;

        public PeliculasController(IPeliculaRepositorio pelRepo, IMapper mapper)
        {
            _pelRepo = pelRepo;
            _mapper = mapper;
        }
        //V1
        //[AllowAnonymous]
        //[HttpGet]
        //[ResponseCache(CacheProfileName = "CachePorDefecto")]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //public IActionResult GetPeliculas()
        //{
        //    var listaPeliculas = _pelRepo.GetPeliculas();
        //    var listaPeliculasDto = new List<PeliculaDto>();

        //    foreach (var lista in listaPeliculas)
        //    {
        //        listaPeliculasDto.Add(_mapper.Map<PeliculaDto>(lista));
        //    }

        //    return Ok(listaPeliculasDto);
        //}

        //V2 con paginación
        [AllowAnonymous]
        [HttpGet]
        [ResponseCache(CacheProfileName = "CachePorDefecto")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetPeliculas([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var totalPeliculas = _pelRepo.GetTotalPeliculas();
                var peliculas = _pelRepo.GetPeliculas(pageNumber,pageSize);

                if (peliculas == null || !peliculas.Any())
                {
                    return NotFound("No se encontraron películas.");
                }

                var peliculasDto = peliculas.Select(p => _mapper.Map<PeliculaDto>(p)).ToList();

                var response = new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPaginas = (int)Math.Ceiling(totalPeliculas / (double)pageSize),
                    TotalItems = totalPeliculas,
                    Items = peliculasDto
                };

                return Ok(response);
            }
            catch (Exception ex) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error recuperando datos de la aplicación.");
            }
        }

        [AllowAnonymous]
        //Es una buena práctica declarar de manera explicita el parámetro que recibe el método
        //y como se va llamar dicho parámetro
        [HttpGet("{peliculaId:int}", Name = "GetPelicula")]
        [ResponseCache(CacheProfileName = "CachePorDefecto")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPelicula(int peliculaId)
        {
            var itemPelicula = _pelRepo.GetPelicula(peliculaId);

            if (itemPelicula == null)
            {
                return NotFound();
            }
            var itemPeliculaDto = _mapper.Map<PeliculaDto>(itemPelicula);

            return Ok(itemPeliculaDto);
        }

        [HttpPost]
        [ProducesResponseType(201, Type = typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //El atributo [FromForm] nos da una manera para subir la información.
        //La cual es compatible con la subida de imágenes

        public IActionResult CrearPelicula([FromForm] CrearPeliculaDto crearPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (crearPeliculaDto == null)
            {
                return BadRequest(ModelState);
            }

            // Si el nombre de la película existe.
            if (_pelRepo.ExistePelicula(crearPeliculaDto.Nombre))
            {
                ModelState.AddModelError("", "La película ya existe");
                return StatusCode(404, ModelState);
            }

            var pelicula = _mapper.Map<Pelicula>(crearPeliculaDto);

            // Inicializa una lista para las rutas de las imágenes
            pelicula.RutasImagenes = new List<string>();
            pelicula.RutasLocalesImagenes = new List<string>();

            // Subida de los archivos
            if (crearPeliculaDto.Imagenes != null && crearPeliculaDto.Imagenes.Count > 0)
            {
                foreach (var imagen in crearPeliculaDto.Imagenes)
                {
                    string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                    string rutaArchivo = @"wwwroot\\ImagenesPeliculas\" + nombreArchivo;

                    // Para obtener el directorio en donde se guardarán las imágenes
                    var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                    FileInfo file = new FileInfo(ubicacionDirectorio);

                    if (System.IO.File.Exists(ubicacionDirectorio))
                    {
                        System.IO.File.Delete(ubicacionDirectorio);
                    }

                    using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                    {
                        imagen.CopyTo(fileStream);
                    }

                    // Construir la URL base del sitio web actual en una aplicación ASP.NET Core.
                    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";

                    // Agregar la ruta de la imagen a las listas
                    pelicula.RutasImagenes.Add(baseUrl + "/ImagenesPeliculas/" + nombreArchivo);
                    pelicula.RutasLocalesImagenes.Add(rutaArchivo);
                }
            }
            else
            {
                pelicula.RutasImagenes.Add("https://placeholder.co/600x400");
            }

            _pelRepo.CrearPelicula(pelicula);

            // Esta línea de código nos sirve para, a la hora de la creación del registro, poder visualizar sus datos como retorno.
            return CreatedAtRoute("GetPelicula", new { peliculaId = pelicula.Id }, pelicula);
        }


        //Solo soporta una imagen
        //public IActionResult CrearPelicula([FromForm] CrearPeliculaDto crearPeliculaDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (crearPeliculaDto == null)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    //Si el nombre de la película existe.
        //    if (_pelRepo.ExistePelicula(crearPeliculaDto.Nombre))
        //    {
        //        ModelState.AddModelError("", "La película ya existe");
        //        return StatusCode(404, ModelState);
        //    }

        //    var pelicula = _mapper.Map<Pelicula>(crearPeliculaDto);

        //    //Subida del archivo
        //    if (crearPeliculaDto.Imagen != null)
        //    {
        //        string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(crearPeliculaDto.Imagen.FileName);
        //        string rutaArchivo = @"wwwroot\\ImagenesPeliculas\" + nombreArchivo;

        //        //Para obtener directorio en donde se guardaran las imégenes
        //        var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(),rutaArchivo);

        //        FileInfo file = new FileInfo(ubicacionDirectorio);

        //        if (System.IO.File.Exists(ubicacionDirectorio))
        //        {
        //            System.IO.File.Delete(ubicacionDirectorio);
        //        }

        //        using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
        //        {
        //            crearPeliculaDto.Imagen.CopyTo(fileStream);
        //        }

        //        //construir la URL base del sitio web actual en una aplicación ASP.NET Core.
        //        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";

        //        pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
        //        pelicula.RutaLocalImagen = rutaArchivo;
        //    }
        //    else
        //    {
        //        pelicula.RutaImagen = "https://placeholder.co/600x400";
        //    }

        //    _pelRepo.CrearPelicula(pelicula);
        //    //Esta línea de código nos sirve para a la hora de la creación del registro poder visualizar sus datos como retorno
        //    return CreatedAtRoute("GetPelicula", new { peliculaId = pelicula.Id }, pelicula);
        //}

        [HttpPatch("{peliculaId:int}", Name = "ActualizarPatchPelicula")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarPatchPelicula(int peliculaId, [FromForm] ActualizarPeliculaDto actualizarPeliculaDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (actualizarPeliculaDto == null || peliculaId != actualizarPeliculaDto.Id)
            {
                return BadRequest(ModelState);
            }

            var peliculaExistente = _pelRepo.GetPelicula(peliculaId);

            if (peliculaExistente == null)
            {
                return NotFound($"No se encontro la película con ID {peliculaId}");
            }

            var pelicula = _mapper.Map<Pelicula>(actualizarPeliculaDto);

            //Subida del archivo
            if (actualizarPeliculaDto.Imagen != null)
            {
                string nombreArchivo = pelicula.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(actualizarPeliculaDto.Imagen.FileName);
                string rutaArchivo = @"wwwroot\\ImagenesPeliculas\" + nombreArchivo;

                //Para obtener directorio en donde se guardaran las imágenes
                var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                FileInfo file = new FileInfo(ubicacionDirectorio);

                //Recordar buscar la manera de cuando actualicen la película se elimine la imágen anterior
                //que hace referencia al id de la película según su primer número que tiene en el nombre de la misma
                if (System.IO.File.Exists(ubicacionDirectorio))
                {
                    System.IO.File.Delete(ubicacionDirectorio);
                }

                using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                {
                    actualizarPeliculaDto.Imagen.CopyTo(fileStream);
                }

                //construir la URL base del sitio web actual en una aplicación ASP.NET Core.
                var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";

                pelicula.RutaImagen = baseUrl + "/ImagenesPeliculas/" + nombreArchivo;
                pelicula.RutaLocalImagen = rutaArchivo;
            }
            else
            {
                pelicula.RutaImagen = "https://placeholder.co/600x400";
            }

            _pelRepo.ActualizarPelicula(pelicula);

            return NoContent();
        }

        [HttpDelete("{peliculaId:int}", Name = "BorrarPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BorrarPelicula(int peliculaId)
        {
            if (!_pelRepo.ExistePelicula(peliculaId))
            {
                return NotFound();

            }
            var pelicula = _pelRepo.GetPelicula(peliculaId);

            if (!_pelRepo.BorrarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Algo salio mal borrando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("GetPeliculasEnCategoria/{categoriaId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult GetPeliculasEnCategoria(int categoriaId)
        {
            try
            {
                var listaPeliculas = _pelRepo.GetPeliculasEnCategoria(categoriaId);

                if (listaPeliculas == null || !listaPeliculas.Any())
                {
                    return NotFound($"No se encontraron películas en la categoría con ID {categoriaId}.");
                }

                //Mejor protección para la propiedad del modelo de películas
                var itemPelicula = listaPeliculas.Select(pelicula => _mapper.Map<PeliculaDto>(pelicula)).ToList();

                return Ok(itemPelicula);
            }
            catch (Exception ex) 
            {
                return StatusCode(StatusCodes.Status500InternalServerError,"Error recuperando datos de la aplicación");
            }
        }

        [AllowAnonymous]
        [HttpGet("Buscar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult Buscar(string nombre)
        {
            try
            {
                var peliculas = _pelRepo.BuscarPelicula(nombre);
                //El método Any nos permite hacer una comparación de lo que el usuario nos envía con lo que esta en la BD coincida
                //Este método es de Entity Framework Core.
                if (!peliculas.Any())
                {
                    return NotFound($"No se encontron películas que coincidan con los criterios de búsqueda: {nombre}.");
                }
                
                var peliculasDto = _mapper.Map<IEnumerable<PeliculaDto>>(peliculas);

                return Ok(peliculasDto);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error recuperando datos de la aplicación.");
            }
        }
    }
}
