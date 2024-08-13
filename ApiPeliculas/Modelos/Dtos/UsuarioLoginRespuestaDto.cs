namespace ApiPeliculas.Modelos.Dtos
{
    //Este Dto nos sirve para traer los datos del usuario una vez está autenticado.
    public class UsuarioLoginRespuestaDto
    {
        public Usuario Usuario { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }
}
