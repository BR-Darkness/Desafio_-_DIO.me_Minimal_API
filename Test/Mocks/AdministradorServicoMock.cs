using MinimalAPI.Dominio.DTOs;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Interfaces;

namespace Test.Mocks
{
    public class AdministradorServicoMock : IAdministradorServico
    {
        private static List<Administrador> administradores = new()
        {
            new Administrador
            {
                Id = 1,
                Email = "adm@teste.com",
                Senha = "123456",
                Perfil = "Administrador"
            },
            new Administrador
            {
                Id = 2,
                Email = "editor@teste.com",
                Senha = "123456",
                Perfil = "Editor"
            }
        };

        public Administrador? BuscaPorId(int id)
        {
            return administradores.Find(administrador => administrador.Id == id);
        }

        public Administrador Incluir(Administrador administrador)
        {
            administrador.Id = administradores.Count() + 1;
            administradores.Add(administrador);

            return administrador;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return administradores.Find(administrador => 
                administrador.Email == loginDTO.Email &&
                administrador.Senha == loginDTO.Senha
            );
        }

        public List<Administrador> Todos(int? pagina)
        {
            return administradores;
        }
    }
}