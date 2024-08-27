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
            if (crearPeliculaDto.Imagenes != null && crearPeliculaDto.Imagenes.Count > 0 && crearPeliculaDto.Imagenes.Count <= 3)
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
                return NotFound($"Has sobrepasado el límite de imágenes");
                //pelicula.RutasImagenes.Add("https://placeholder.co/600x400");
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
                return NotFound($"No se encontró la película con ID {peliculaId}");
            }

            // Inicializa las listas de rutas de imágenes en caso de que no estén inicializadas
            peliculaExistente.RutasImagenes ??= new List<string>();
            peliculaExistente.RutasLocalesImagenes ??= new List<string>();

            // Subida de los nuevos archivos
            if (actualizarPeliculaDto.Imagenes != null && actualizarPeliculaDto.Imagenes.Count > 0 && actualizarPeliculaDto.Imagenes.Count <= 3)
            {
                for (int i = 0; i < actualizarPeliculaDto.Imagenes.Count; i++)
                {
                    var imagen = actualizarPeliculaDto.Imagenes[i];

                    // Comprobar si el archivo es distinto al que ya está almacenado
                    string nombreArchivoNuevo = peliculaExistente.Id + System.Guid.NewGuid().ToString() + Path.GetExtension(imagen.FileName);
                    //Optiene el archivo que está supuesto a existir en la BD, si existe dicho archivo en dicha posición accede a el, de lo contrario envía una cadena vacía
                    string nombreArchivoExistente = Path.GetFileName(peliculaExistente.RutasLocalesImagenes.Count > i ? peliculaExistente.RutasLocalesImagenes[i] : "");

                    // Si el nombre del archivo es diferente, o no hay archivo en esa posición, entonces se actualiza
                    if (nombreArchivoExistente != imagen.FileName)
                    {
                        string rutaArchivo = @"wwwroot\\ImagenesPeliculas\" + nombreArchivoNuevo;
                        var ubicacionDirectorio = Path.Combine(Directory.GetCurrentDirectory(), rutaArchivo);

                        // Eliminar la imagen antigua en esa posición si existe
                        if (peliculaExistente.RutasLocalesImagenes.Count > i && System.IO.File.Exists(peliculaExistente.RutasLocalesImagenes[i]))
                        {
                            System.IO.File.Delete(peliculaExistente.RutasLocalesImagenes[i]);
                        }

                        // Guardar la nueva imagen
                        using (var fileStream = new FileStream(ubicacionDirectorio, FileMode.Create))
                        {
                            imagen.CopyTo(fileStream);
                        }

                        // Construir la URL base del sitio web actual
                        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";

                        // Actualizar las listas con las rutas de las nuevas imágenes
                        if (peliculaExistente.RutasImagenes.Count > i)
                        {
                            peliculaExistente.RutasImagenes[i] = baseUrl + "/ImagenesPeliculas/" + nombreArchivoNuevo;
                            peliculaExistente.RutasLocalesImagenes[i] = rutaArchivo;
                        }
                        else
                        {
                            peliculaExistente.RutasImagenes.Add(baseUrl + "/ImagenesPeliculas/" + nombreArchivoNuevo);
                            peliculaExistente.RutasLocalesImagenes.Add(rutaArchivo);
                        }
                    }
                }
            }
            else
            {
                return BadRequest("Has sobrepasado el límite de imágenes o no se han proporcionado imágenes.");
            }

            _pelRepo.ActualizarPelicula(peliculaExistente);

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

            if (pelicula.RutasLocalesImagenes != null && pelicula.RutasLocalesImagenes.Count > 0)
            {
                foreach (var rutaLocalImagen in pelicula.RutasLocalesImagenes)
                {
                    if (System.IO.File.Exists(rutaLocalImagen))
                    {
                        System.IO.File.Delete(rutaLocalImagen);
                    }
                }
            }

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
