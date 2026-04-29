using ApiExamenCubosJorgeRodriguez.Helpers;
using ApiExamenCubosJorgeRodriguez.Models;
using ApiExamenCubosJorgeRodriguez.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiExamenCubosJorgeRodriguez.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private RepositoryCubos repo;
        private HelperActionOAuthService helper;

        public AuthController(RepositoryCubos repo, HelperActionOAuthService helper)
        {
            this.repo = repo;
            this.helper = helper;
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> Login(LoginModel model)
        {
            Usuario user = await this.repo.LogInUsuarioAsync(model.Email, model.Password);
            if (user == null)
            {
                return Unauthorized();
            }
            else
            {
                SigningCredentials credentials = new SigningCredentials(this.helper.GetKeyToken(), SecurityAlgorithms.HmacSha256);

                string jsonUsuario = JsonConvert.SerializeObject(user);

                string jsonCifrado = HelperCifrado.Encrypt(jsonUsuario);

                Claim[] info = new[]
                {
                    new Claim("UserData", jsonCifrado),
                };

                //GENERAR TOKEN
                JwtSecurityToken token = new JwtSecurityToken(
                    claims: info,
                    issuer: this.helper.Issuer,
                    audience: this.helper.Audience,
                    signingCredentials: credentials,
                    expires: DateTime.UtcNow.AddMinutes(20),
                    notBefore: DateTime.UtcNow
                    );

                return Ok(new
                {
                    response = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
        }

    }
}
