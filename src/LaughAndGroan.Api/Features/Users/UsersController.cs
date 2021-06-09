namespace LaughAndGroan.Api.Features.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    [Route("users")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _users;
        private readonly ILogger<UsersController> _log;

        public UsersController(UsersService users, ILogger<UsersController> log)
        {
            _users = users;
            _log = log;
        }

        [HttpGet("me"), Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<UserApiData>> GetMe(CancellationToken cancellationToken)
        {
            _log.LogWarning("Claims {@claims}", HttpContext.User.Claims.Select(c => new KeyValuePair<string, string>(c.Type, c.Value)));

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
