using ApiPeliculas.Data;
using ApiPeliculas.Modelos;
using ApiPeliculas.Repositorio.IRepositorio;
using Microsoft.EntityFrameworkCore;

namespace ApiPeliculas.Repositorio
{
    public class PeliculaRepositorio : IPeliculaRepositorio
    {
        private readonly ApplicationDbContext _bd;

        public PeliculaRepositorio(ApplicationDbContext bd)
        {
            _bd = bd;
        }

        public bool ActualizarPelicula(Pelicula pelicula)
        {
            //Arreglar problema del PATCH
            pelicula.FechaCreacion = DateTime.Now;
            var peliculaExistente = _bd.Pelicula.Find(pelicula.Id);
            if (peliculaExistente != null)
            {
                _bd.Entry(peliculaExistente).CurrentValues.SetValues(pelicula);
            }
            else
            {
                _bd.Pelicula.Update(pelicula);
            }
            return Guardar();
        }

        public bool BorrarPelicula(Pelicula pelicula)
        {
            _bd.Pelicula.Remove(pelicula);
            return Guardar();
        }

        public IEnumerable<Pelicula> BuscarPelicula(string nombre)
        {
            //IQueryable sirva para poder hacer un query sobre una entidad
            IQueryable<Pelicula> query = _bd.Pelicula;
            if (!string.IsNullOrEmpty(nombre))
            {
                //Aquí filtramos una busqueda para que busque en el Nombre y Descripción los elementos
                //que contengan dicho nombre
                query = query.Where(e => e.Nombre.Contains(nombre) || e.Descripcion.Contains(nombre));
            }
            //Ahora retornamos el resultado lo hacemos en forma de lista porque pueden venir varios.
            return query.ToList();
        }

        public bool CrearPelicula(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;
            _bd.Pelicula.Add(pelicula);
            return Guardar();
        }

        public bool ExistePelicula(int id)
        {
            return _bd.Pelicula.Any(p => p.Id == id);
        }

        public bool ExistePelicula(string nombre)
        {
            bool valor = _bd.Pelicula.Any(p => p.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            return valor;
        }

        public Pelicula GetPelicula(int peliculaId)
        {
            //Retorna la información del registro basado en el ID
            return _bd.Pelicula.FirstOrDefault(p => p.Id == peliculaId);
        }

        public ICollection<Pelicula> GetPeliculas()
        {
            return _bd.Pelicula.OrderBy(p => p.Nombre).ToList();
        }

        public ICollection<Pelicula> GetPeliculasEnCategoria(int catId)
        {
            return _bd.Pelicula.Include(ca => ca.Categoria).Where(ca => ca.categoriaId == catId).ToList();
        }

        public bool Guardar()
        {
            return _bd.SaveChanges() >= 0 ? true : false;
        }
    }
}
