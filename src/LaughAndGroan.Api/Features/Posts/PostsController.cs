namespace LaughAndGroan.Api.Features.Posts
{
    using System.Linq;
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Mime;
    using System.Threading;
    using System.Threading.Tasks;

    [Route("posts")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly PostsService _posts;

        public PostsController(PostsService posts)
        {
            _posts = posts;
        }

        [HttpPost(""), Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PostData>> Create([FromBody] PostApiRequest request)
        {
            var userId = HttpContext.User.ExtractUserId();
            if (userId == null)
                return Unauthorized();

            var postCreated = await _posts.CreatePost(userId, request.Url, request.Title);
            
            return Ok(postCreated);
        }

        [HttpGet(""), Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<GetPostsResponse>> GetPosts([FromQuery(Name = "by")] string userName,
            [FromQuery(Name = "from")] string fromPostId, CancellationToken cancellationToken)
        {
            var result = await _posts.GetPosts(fromPostId, userName == null ? null : new[] { userName }, cancellationToken: cancellationToken);
            var response = new GetPostsResponse()
            {
                Data = result.Take(25).Select(p => new PostApiResponse(p)).ToArray()
            };

            return Ok(response);
        }

        [HttpGet("{postId}"), Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PostApiResponse>> GetPost([FromRoute] string postId,
            CancellationToken cancellationToken)
        {
            var postFound = await _posts.GetPost(postId, cancellationToken);
            if (postFound == null)
                return NotFound();

            return Ok(postFound);
        }

        [HttpDelete("{postId}")]
        public async Task<ActionResult> DeletePost([FromRoute] string postId, CancellationToken cancellationToken)
        {
            var userId = HttpContext.User.ExtractUserId();
            if (userId == null)
                return Unauthorized();

            if (!await _posts.DeletePost(userId, postId, cancellationToken))
                return Unauthorized();

            return NoContent();
        }
    }
}
