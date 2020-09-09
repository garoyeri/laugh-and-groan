namespace LaughAndGroan.Actions.Posts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.DataModel;
    using NUlid;
    using NUlid.Rng;

    public class PostsService
    {
        private readonly MonotonicUlidRng _random;
        private readonly DynamoDBContext _context;

        public PostsService(Settings settings = null)
        {
            settings ??= new ConfigurationProvider().Settings;
            _random = new MonotonicUlidRng();

            var client = settings.DynamoDbUrl == null
                ? new AmazonDynamoDBClient()
                : new AmazonDynamoDBClient(new AmazonDynamoDBConfig {ServiceURL = settings.DynamoDbUrl});
            var contextConfig = new DynamoDBContextConfig()
            {
                TableNamePrefix = settings.TableNamePrefix,
            };
            _context = new DynamoDBContext(client, contextConfig);
        }

        public async Task<PostData> CreatePost(string userId, string url, DateTimeOffset? timestamp = null, CancellationToken cancellationToken = default)
        {
            timestamp ??= DateTimeOffset.UtcNow;
            var postId = Ulid.NewUlid(timestamp.Value.ToUniversalTime(), _random);

            var postData = new PostData { PostId = postId.ToString(), Url = url, UserId = userId};
            await _context.SaveAsync(postData, cancellationToken);

            return postData;
        }

        public async Task<PostData> GetPost(string userId, string postId, CancellationToken cancellationToken = default)
        {
            var postFound = await _context.LoadAsync<PostData>(userId, postId, cancellationToken);
            return postFound;
        }

        public Task DeletePost(string userId, string postId, CancellationToken cancellationToken = default)
        {
            return _context.DeleteAsync<PostData>(userId, postId, cancellationToken);
        }
    }
}
