using ApiExamenCubosJorgeRodriguez.Helpers;
using ApiExamenCubosJorgeRodriguez.Models;
using ApiExamenCubosJorgeRodriguez.Repositories;
using ApiExamenCubosJorgeRodriguez.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace ApiExamenCubosJorgeRodriguez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CubosController : ControllerBase
    {
        private RepositoryCubos repo;
        private BlobService service;

        public CubosController(RepositoryCubos repo, BlobService service)
        {
            this.repo = repo;
            this.service = service;
        }

        // ==========================================
        // MÉTODOS PÚBLICOS (SIN TOKEN)
        // ==========================================

        [HttpGet]
        public async Task<ActionResult<List<Cubo>>> GetCubos()
        {
            var cubos = await this.repo.GetCubosAsync();
            foreach (var cubo in cubos)
            {
                cubo.Imagen = this.service.GetBlobUrl("cubos", cubo.Imagen) ?? cubo.Imagen;
            }
            return cubos;
        }

        [HttpGet("Marca/{marca}")]
        public async Task<ActionResult<List<Cubo>>> FindCubosByMarca(string marca)
        {
            var cubos = await this.repo.FindCuboByMarcaAsync(marca);
            foreach (var cubo in cubos)
            {
                cubo.Imagen = this.service.GetBlobUrl("cubos", cubo.Imagen) ?? cubo.Imagen;
            }
            return cubos;
        }

        [HttpPost("Registro")]
        public async Task<ActionResult> Registro([FromForm] Usuario usuario, IFormFile imagen)
        {
            if (imagen != null)
            {
                string fileName = imagen.FileName;

                using (var stream = imagen.OpenReadStream())
                {
                    // Subimos al contenedor "usuarios" (que debe estar configurado como Privado en Azure)
                    await this.service.UploadBlobAsync("usuarios", fileName, stream);
                }
                usuario.Imagen = fileName;
            }

            await this.repo.InsertUsuarioAsync(usuario);
            return Ok();
        }


        // ==========================================
        // MÉTODOS PROTEGIDOS (CON TOKEN)
        // ==========================================

        [Authorize]
        private Usuario GetUsuarioFromToken()
        {
            Claim claim = HttpContext.User.FindFirst("UserData");
            if (claim != null)
            {
                string jsonCifrado = claim.Value;
                // Usamos la misma clase que usaste para cifrar en el AuthController
                string jsonUsuario = HelperCifrado.Decrypt(jsonCifrado);
                Usuario user = JsonConvert.DeserializeObject<Usuario>(jsonUsuario);
                return user;
            }
            return null;
        }

        [Authorize]
        [HttpGet("Perfil")]
        public async Task<ActionResult<Usuario>> PerfilUsuario()
        {
            Usuario userToken = GetUsuarioFromToken();
            if (userToken == null) return Unauthorized();

            Usuario userDb = await this.repo.FindUsuarioAsync(userToken.IdUsuario);
            if (userDb != null && !string.IsNullOrEmpty(userDb.Imagen))
            {
                userDb.Imagen = this.service.GetProtectedBlobUrl("usuarios", userDb.Imagen) ?? userDb.Imagen;
            }
            return Ok(userDb);
        }

        [Authorize]
        [HttpGet("Pedidos")]
        public async Task<ActionResult<List<Compra>>> PedidosUsuario()
        {
            Usuario userToken = GetUsuarioFromToken();
            if (userToken == null) return Unauthorized();

            List<Compra> pedidos = await this.repo.GetPedidosUsuarioAsync(userToken.IdUsuario);
            return Ok(pedidos);
        }

        [Authorize]
        [HttpPost("RealizarPedido/{idCubo}")]
        public async Task<ActionResult> RealizarPedido(int idCubo)
        {
            Usuario userToken = GetUsuarioFromToken();
            if (userToken == null) return Unauthorized();

            await this.repo.InsertPedidoAsync(idCubo, userToken.IdUsuario);
            return Ok(new { mensaje = "Pedido realizado con éxito" });
        }
    }
}
