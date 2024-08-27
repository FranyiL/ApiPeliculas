using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPeliculas.Modelos.Dtos
{
    public class ActualizarPeliculaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Duracion { get; set; }
        public List<string>? RutasImagenes { get; set; }
        public List<string>? RutasLocalesImagenes { get; set; }
        public List<IFormFile> Imagenes { get; set; }
        public enum TipoClasificion { Siete, Trece, Dieciseis, Dieciocho }
        public TipoClasificion Clasificacion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public int categoriaId { get; set; }
    }
}
