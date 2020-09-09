namespace LaughAndGroan.Actions.Users
{
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.DataModel;

    public class UsersService
    {
        private readonly DynamoDBContext _context;
        private readonly AmazonDynamoDBClient _client;

        public UsersService(Settings settings = null)
        {
            settings ??= new ConfigurationProvider().Settings;

            _client = settings.DynamoDbUrl == null
                ? new AmazonDynamoDBClient()
                : new AmazonDynamoDBClient(new AmazonDynamoDBConfig {ServiceURL = settings.DynamoDbUrl});
            var contextConfig = new DynamoDBContextConfig()
            {
                TableNamePrefix = settings.TableNamePrefix,
                ConsistentRead = true,
            };
            _context = new DynamoDBContext(_client, contextConfig);
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
    }
}
