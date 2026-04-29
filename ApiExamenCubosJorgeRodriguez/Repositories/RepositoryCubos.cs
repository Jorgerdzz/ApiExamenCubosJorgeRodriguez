using ApiExamenCubosJorgeRodriguez.Data;
using ApiExamenCubosJorgeRodriguez.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiExamenCubosJorgeRodriguez.Repositories
{
    public class RepositoryCubos
    {
        private CubosContext context;

        public RepositoryCubos(CubosContext context)
        {
            this.context = context;
        }

        public async Task<List<Cubo>> GetCubosAsync()
        {
            return await this.context.Cubos.ToListAsync();
        }

        public async Task<List<Cubo>> FindCuboByMarcaAsync(string marca)
        {
            return await this.context.Cubos
                .Where(c => c.Marca == marca)
                .ToListAsync();
        }


        public async Task<Usuario> LogInUsuarioAsync(string email, string password)
        {
            return await this.context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);
        }

        public async Task<List<Compra>> GetPedidosUsuarioAsync(int idUsuario)
        {
            return await this.context.Compras
                .Where(c => c.IdUsuario == idUsuario)
                .ToListAsync();
        }

        public async Task<Usuario> FindUsuarioAsync(int idUsuario)
        {
            return await this.context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
        }

        public async Task InsertUsuarioAsync(Usuario usuario)
        {
            // Buscamos el ID más alto para autoincrementar (ya que no es identity en tu script SQL)
            int maxId = await this.context.Usuarios.MaxAsync(u => (int?)u.IdUsuario) ?? 0;
            usuario.IdUsuario = maxId + 1;

            this.context.Usuarios.Add(usuario);
            await this.context.SaveChangesAsync();
        }

        public async Task InsertPedidoAsync(int idCubo, int idUsuario)
        {
            int maxId = await this.context.Compras.MaxAsync(c => (int?)c.IdPedido) ?? 0;

            Compra pedido = new Compra
            {
                IdPedido = maxId + 1,
                IdCubo = idCubo,
                IdUsuario = idUsuario,
                FechaPedido = DateTime.UtcNow // Fecha actual
            };

            this.context.Compras.Add(pedido);
            await this.context.SaveChangesAsync();
        }

    }
}
