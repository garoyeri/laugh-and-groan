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
                ConsistentRead = true,
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

        public async Task<PostData> GetPost(string postId, CancellationToken cancellationToken = default)
        {
            var postFound = await _context.LoadAsync<PostData>(postId, cancellationToken);
            return postFound;
        }

        public async Task<bool> DeletePost(string userId, string postId, CancellationToken cancellationToken = default)
        {
            var postFound = await GetPost(postId, cancellationToken);
            if (postFound == null)
                return true;
            if (postFound.UserId != userId)
                return false;

            await _context.DeleteAsync<PostData>(postId, cancellationToken);

            return true;
        }
    }
}
