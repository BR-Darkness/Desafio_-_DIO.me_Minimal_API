using Microsoft.EntityFrameworkCore;
using MinimalAPI.Dominio.Entidades;
using MinimalAPI.Dominio.Interfaces;
using MinimalAPI.DTOs;
using MinimalAPI.Infraestrutura.Db;

namespace MinimalAPI.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorServico
    {
        private readonly DbContexto _contexto;

        public AdministradorServico(DbContexto contexto)
        {
            _contexto = contexto;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            return _contexto.Administradores.Where(adm =>
                adm.Email == loginDTO.Email &&
                adm.Senha == loginDTO.Senha
            ).FirstOrDefault();
        }
    }
}