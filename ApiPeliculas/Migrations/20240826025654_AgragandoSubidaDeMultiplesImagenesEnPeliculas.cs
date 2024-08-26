using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiPeliculas.Migrations
{
    /// <inheritdoc />
    public partial class AgragandoSubidaDeMultiplesImagenesEnPeliculas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RutaLocalImagen",
                table: "Pelicula",
                newName: "RutasLocalesImagenes");

            migrationBuilder.RenameColumn(
                name: "RutaImagen",
                table: "Pelicula",
                newName: "RutasImagenes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RutasLocalesImagenes",
                table: "Pelicula",
                newName: "RutaLocalImagen");

            migrationBuilder.RenameColumn(
                name: "RutasImagenes",
                table: "Pelicula",
                newName: "RutaImagen");
        }
    }
}
