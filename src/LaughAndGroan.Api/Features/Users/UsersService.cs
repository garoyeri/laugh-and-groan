namespace LaughAndGroan.Api.Features.Users
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2.DataModel;

    public class UsersService
    {
        private readonly IDynamoDBContext _context;

        public UsersService(IDynamoDBContext context)
        {
            _context = context;
        }

        public async Task<UserData> Create(string userId, string userName = null,
            CancellationToken cancellationToken = default)
        {
            userName ??= NameGenerator.GetRandomName(100);
            var userData = new UserData { UserId = userId, UserName = userName };

            await _context.SaveAsync(userData, cancellationToken);

            return userData;
        }

        public Task<UserData> Get(string userName, CancellationToken cancellationToken = default)
        {
            return _context.LoadAsync<UserData>(userName, cancellationToken);
        }

        public async Task<List<UserData>> Get(IEnumerable<string> userNames, CancellationToken cancellationToken = default)
        {
            if (!userNames.Any())
                return new List<UserData>(0);

            var batch = _context.CreateBatchGet<UserData>();
            foreach (var name in userNames)
                batch.AddKey(name);

            await batch.ExecuteAsync(cancellationToken);
            return batch.Results;
        }

        public async Task<UserData> GetById(string userId, CancellationToken cancellationToken = default)
        {
            var query = _context.QueryAsync<UserData>(userId, new DynamoDBOperationConfig
            {
                IndexName = "UserIdIndex",
                ConsistentRead = false
            });

            var results = await query.GetNextSetAsync(cancellationToken);
            if (results.Any())
                return results.First();

            return await Create(userId, cancellationToken: cancellationToken);
        }
    }
}
