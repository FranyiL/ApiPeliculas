using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPeliculas.Modelos.Dtos
{
    public class PeliculaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Duracion { get; set; }
        public string RutaImagen { get; set; }
        public enum TipoClasificion { Siete, Trece, Dieciseis, Dieciocho }
        public TipoClasificion Clasificacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public int categoriaId { get; set; }
    }
}
