using ApiPeliculas.Modelos;

namespace ApiPeliculas.Repositorio.IRepositorio
{
    public interface ICategoriaRepositorio
    {
        //Método para traer el listado de categorías
        ICollection<Categoria> GetCategorias();
        //Obtener una sola categoría
        Categoria GetCategoria(int CategoriaId);
        //Método para verificar la existencia de una categoría por id
        bool ExisteCategoria(int id);
        //Método para verificar la existencia de una categoría por nombre
        bool ExisteCategoria(string nombre);
        //Método para crear categoría
        bool CrearCategoria(Categoria categoria);
        //Método para actualizar categoría
        bool ActualizarCategoria(Categoria categoria);
        //Método para borrar categoría
        bool BorrarCategoria(Categoria categoria);
        //Método para guardar los cambios
        bool Guardar();
    }
}
