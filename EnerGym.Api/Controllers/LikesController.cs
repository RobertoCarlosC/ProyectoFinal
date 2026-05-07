using Microsoft.AspNetCore.Mvc;
using EnerGym.Services.Interfaces;

namespace EnerGym.Controllers
{
    [ApiController]
    [Route("api/likes")]
    public class LikesController : BaseApiController
    {
        private readonly ILikeService _likeService;

        public LikesController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleLike([FromBody] Models.LikeDto dto)
        {
            var liked = await _likeService.ToggleAsync(dto.IdUsuario, dto.IdProducto);
            return OkData(new { liked, message = liked ? "Like añadido." : "Like eliminado." });
        }

        [HttpGet("producto/{idProducto:int}/count")]
        public async Task<IActionResult> GetLikesProducto(int idProducto)
        {
            var total = await _likeService.CountByProductoAsync(idProducto);
            return OkData(new { idProducto, totalLikes = total });
        }

        [HttpGet("{idUsuario:int}")]
        public async Task<IActionResult> GetLikesUsuario(int idUsuario)
        {
            var likes = await _likeService.GetByUsuarioAsync(idUsuario);
            return OkData(likes);
        }
    }
}
