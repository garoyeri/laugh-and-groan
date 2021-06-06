using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaughAndGroan.Api.Features.Posts
{
    using System.Net.Mime;

    [Route("posts")]
    [ApiController]
    public class PostsController : ControllerBase
    {
        [HttpPost(""), Produces(MediaTypeNames.Application.Json)]
        public async Task<ActionResult<PostData>> Create([FromBody] PostApiRequest request)
        {
            return Ok(new PostData());
        }


    }
}
