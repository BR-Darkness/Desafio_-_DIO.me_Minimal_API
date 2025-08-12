using MinimalAPI.Dominio.Enuns;

namespace MinimalAPI.Dominio.DTOs
{
    public class AdministradorDTO
    {
        public string Email { get; set; } = default!;
        public string Senha { get; set; } = default!;
        public TipoPerfil? Perfil { get; set; } = default!;
    }
}