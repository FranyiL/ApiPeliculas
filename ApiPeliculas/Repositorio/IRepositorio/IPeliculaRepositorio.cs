using ApiPeliculas.Modelos;

namespace ApiPeliculas.Repositorio.IRepositorio
{
    public interface IPeliculaRepositorio
    {
        //Método para traer el listado de peliculas v1
        //ICollection<Pelicula> GetPeliculas();

        //Método para traer el listado de peliculas v2 paginación
        ICollection<Pelicula> GetPeliculas(int pageNumber, int pageSize);
        //Contador para las películas
        int GetTotalPeliculas();
        //Método para traer el listado de peliculas por su categoría
        ICollection<Pelicula> GetPeliculasEnCategoria(int catId);
        //Buscar pelicula por nombre
        IEnumerable<Pelicula> BuscarPelicula(string nombre);
        //Obtener una sola pelicula
        Pelicula GetPelicula(int PeliculaId);
        //Método para verificar la existencia de una pelicula por id
        bool ExistePelicula(int id);
        //Método para verificar la existencia de una pelicula por nombre
        bool ExistePelicula(string nombre);
        //Método para crear pelicula
        bool CrearPelicula(Pelicula pelicula);
        //Método para actualizar pelicula
        bool ActualizarPelicula(Pelicula pelicula);
        //Método para borrar pelicula
        bool BorrarPelicula(Pelicula pelicula);
        //Método para guardar los cambios
        bool Guardar();
    }
}
