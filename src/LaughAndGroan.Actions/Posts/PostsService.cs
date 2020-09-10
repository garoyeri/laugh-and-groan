namespace LaughAndGroan.Actions.Posts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.DataModel;
    using Amazon.DynamoDBv2.DocumentModel;
    using NUlid;
    using NUlid.Rng;
    using Users;

    public class PostsService
    {
        private readonly MonotonicUlidRng _random;
        private readonly DynamoDBContext _context;
        private readonly UsersService _users;
        private readonly Settings _settings;

        public PostsService(Settings settings = null, DynamoDBContext context = null)
        {
            _settings = settings;
            _settings ??= new ConfigurationProvider().Settings;
            _random = new MonotonicUlidRng();

            var client = _settings.DynamoDbUrl == null
                ? new AmazonDynamoDBClient()
                : new AmazonDynamoDBClient(new AmazonDynamoDBConfig {ServiceURL = _settings.DynamoDbUrl});
            var contextConfig = new DynamoDBContextConfig()
            {
                TableNamePrefix = _settings.TableNamePrefix,
                ConsistentRead = true,
            };
            _context = context ?? new DynamoDBContext(client, contextConfig);

            _users = new UsersService(_settings, _context);
        }

        public async Task<PostData> CreatePost(string userId, string url, DateTimeOffset? timestamp = null, CancellationToken cancellationToken = default)
        {
            timestamp ??= DateTimeOffset.UtcNow;
            var postId = Ulid.NewUlid(timestamp.Value.ToUniversalTime(), _random).ToString();

            var postData = new PostData { PostId = postId, Url = url, UserId = userId, Type = "post"};
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

        public async Task<List<PostData>> GetPosts(string fromPostId, IEnumerable<string> byUserNames, int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            byUserNames ??= new string[0];
            var usersFound = await _users.Get(byUserNames, cancellationToken);

            var conditions = new List<ScanCondition>(2);
            if (fromPostId != null)
                conditions.Add(new ScanCondition("PostId", ScanOperator.LessThan, fromPostId));
            if (usersFound.Any())
                conditions.Add(new ScanCondition("UserId", ScanOperator.In, usersFound.Select(u => (object)u.UserId).ToArray()));

            var query = _context.QueryAsync<PostData>("post", new DynamoDBOperationConfig
            {
                TableNamePrefix = _settings.TableNamePrefix,
                ConditionalOperator = ConditionalOperatorValues.And,
                QueryFilter = conditions,
                BackwardQuery = true,
                IndexName = "ChronologicalPostsIndex",
                ConsistentRead = false
            });

            var results = new List<PostData>(pageSize);

            do
            {
                var nextResult = await query.GetNextSetAsync(cancellationToken);
                results.AddRange(nextResult);
            } while (!query.IsDone && results.Count < pageSize);

            return results;
        }
    }
}
