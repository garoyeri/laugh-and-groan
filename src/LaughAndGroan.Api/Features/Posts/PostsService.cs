namespace LaughAndGroan.Api.Features.Posts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.DataModel;
    using Amazon.DynamoDBv2.DocumentModel;
    using Amazon.Runtime;
    using NUlid;
    using NUlid.Rng;
    using Users;

    public class PostsService
    {
        private readonly MonotonicUlidRng _random;
        private readonly DynamoDBContext _context;
        private readonly UsersService _users;

        public PostsService(IDynamoDBContext context)
        {
            _random = new MonotonicUlidRng();

            var client = _settings.DynamoDbUrl == null
                ? new AmazonDynamoDBClient()
                : new AmazonDynamoDBClient(
                    new BasicAWSCredentials("DUMMY", "DUMMY"),  // testing credentials
                    new AmazonDynamoDBConfig { ServiceURL = settings.DynamoDbUrl });
            var contextConfig = new DynamoDBContextConfig()
            {
                TableNamePrefix = _settings.TableNamePrefix,
                ConsistentRead = true,
            };
            _context = context ?? new DynamoDBContext(client, contextConfig);

            _users = new UsersService(_settings, _context);
        }

        public async Task<PostData> CreatePost(string userId, string url, string title = null, DateTimeOffset? timestamp = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId)) throw new ArgumentException("UserId cannot be empty", nameof(userId));
            if (string.IsNullOrEmpty(url)) throw new ArgumentException("Url cannot be empty", nameof(url));
            if ((title?.Length ?? 0) > 240) throw new ArgumentException("Title cannot be more than 240 characters", nameof(title));

            if (!Uri.TryCreate(url, UriKind.Absolute, out var sanitizedUrl))
                throw new ArgumentException("Url must be a valid absolute URL", nameof(url));
            if (sanitizedUrl.Scheme != "https" && sanitizedUrl.Scheme != "http")
                throw new ArgumentException("Url must be an http(s) URL", nameof(url));

            timestamp ??= DateTimeOffset.UtcNow;
            var postId = Ulid.NewUlid(timestamp.Value.ToUniversalTime(), _random).ToString();

            var postData = new PostData { PostId = postId, Url = url, Title = title, UserId = userId, Type = "post"};
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
