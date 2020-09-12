namespace LaughAndGroan.Actions.Posts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Amazon.Lambda.APIGatewayEvents;
    using Amazon.Lambda.Core;
    using Amazon.Lambda.Serialization.SystemTextJson;

    public class PostsHandler
    {
        readonly PostsService _posts = new PostsService();
        readonly ILambdaSerializer _serializer = new CamelCaseLambdaJsonSerializer();

        public async Task<APIGatewayHttpApiV2ProxyResponse> Create(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var claims = request.GetAuthorization();
            if (claims == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401
                };
            }

            try
            {
                var requestData = _serializer.DeserializeObject<PostApiRequest>(request.Body);
                var postCreated = await _posts.CreatePost(claims["sub"], requestData.Url, requestData.Title);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    },
                    Body = _serializer.SerializeObject(new PostApiResponse(postCreated)),
                    StatusCode = 200
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine("ERROR " + e);

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Headers = new Dictionary<string, string>
                    {
                        {"Content-Type", "application/json"}
                    },
                    Body = _serializer.SerializeObject(new PostError { Message = e.Message }),
                    StatusCode = 400,
                };
            }
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> Get(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var claims = request.GetAuthorization();
            if (claims == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401
                };
            }

            try
            {
                if (request.PathParameters.TryGetValue("postId", out var postId))
                {
                    var postFound = await _posts.GetPost(postId);

                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        Headers = new Dictionary<string, string>
                        {
                            { "Content-Type", "application/json" }
                        },
                        Body = _serializer.SerializeObject(new PostApiResponse(postFound)),
                        StatusCode = 200
                    };
                }

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 404
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine("ERROR " + e);
                throw;
            }
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> Delete(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var claims = request.GetAuthorization();
            if (claims == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401
                };
            }

            try
            {
                var userId = claims["sub"];

                if (request.PathParameters.TryGetValue("postId", out var postId))
                {
                    if (!await _posts.DeletePost(userId, postId))
                    {
                        return new APIGatewayHttpApiV2ProxyResponse
                        {
                            StatusCode = 403
                        };
                    }
                }

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 204
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine("ERROR " + e);
                throw;
            }
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> GetPosts(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            var claims = request.GetAuthorization();
            if (claims == null)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 401
                };
            }

            string userName = null;
            string fromPostId = null;

            request.QueryStringParameters?.TryGetValue("by", out userName);
            request.QueryStringParameters?.TryGetValue("from", out fromPostId);

            try
            {
                var result = await _posts.GetPosts(fromPostId, userName == null ? null : new[] { userName });
                var response = new GetPostsResponse()
                {
                    Data = result.Take(25).Select(p => new PostApiResponse(p)).ToArray()
                };

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Headers = new Dictionary<string, string>
                    {
                        { "Content-Type", "application/json" }
                    },
                    StatusCode = 200,
                    Body = _serializer.SerializeObject(response)
                };
            }
            catch (Exception e)
            {
                context.Logger.LogLine("ERROR " + e);
                throw;
            }
        }
    }
}