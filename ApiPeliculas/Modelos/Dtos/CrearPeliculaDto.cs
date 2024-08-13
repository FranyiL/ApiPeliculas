using System.ComponentModel.DataAnnotations.Schema;

namespace ApiPeliculas.Modelos.Dtos
{
    public class CrearPeliculaDto
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Duracion { get; set; }
        public string RutaImagen { get; set; }
        public enum CrearTipoClasificion { Siete, Trece, Dieciseis, Dieciocho }
        public CrearTipoClasificion Clasificacion { get; set; }
        public int categoriaId { get; set; }
    }
}
