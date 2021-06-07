namespace LaughAndGroan.Api.Features.Users
{
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    [Route("users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _users;

        public UsersController(UsersService users)
        {
            _users = users;
        }

        [HttpGet("me"), Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<UserApiData>> GetMe(CancellationToken cancellationToken)
        {
            var userId = HttpContext.User.ExtractUserId();
            if (userId == null)
                return Unauthorized();

            var response = await _users.GetById(userId, cancellationToken);
            return Ok(new UserApiData
            {
                UserName = response.UserName,
                Id = response.UserId
            });
        }
    }
}
