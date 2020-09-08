namespace LaughAndGroan.Actions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.Model;
    using NUlid;
    using Shouldly;

    public class PostsTests
    {
        private readonly Settings _settings = new Settings
        {
            DynamoDbUrl = "http://localhost:8000/",
            TableNamePrefix = "LaughAndGroanLocalTest"
        };
        private readonly PostsService _service;
        private readonly AmazonDynamoDBClient _client;

        public PostsTests()
        {
            _client = new AmazonDynamoDBClient(new AmazonDynamoDBConfig {ServiceURL = _settings.DynamoDbUrl});

            _service = new PostsService(_settings);
        }

        public async Task SetUp()
        {
            // create the tables for testing
            var tableName = _settings.TableNamePrefix + "Posts";
            var postsCreateTableResponse = await _client.CreateTableAsync(tableName, new List<KeySchemaElement>
                {
                    new KeySchemaElement("userId", KeyType.HASH),
                    new KeySchemaElement("postId", KeyType.RANGE)
                },
                new List<AttributeDefinition>
                {
                    new AttributeDefinition("userId", ScalarAttributeType.S),
                    new AttributeDefinition("postId", ScalarAttributeType.S)
                },
                new ProvisionedThroughput(10, 5)
            );

            var status = postsCreateTableResponse.TableDescription.TableStatus;
            for (var i = 0; i < 12 || status != TableStatus.ACTIVE; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
                var response = await _client.DescribeTableAsync(tableName);
                status = response.Table.TableStatus;
            }

            status.ShouldBe(TableStatus.ACTIVE);
        }

        public async Task CanCreatePostAndReadItBack()
        {
            var userId = Ulid.NewUlid();
            var postCreated = await _service.CreatePost(userId.ToString(), "https://localtest.laughandgroan.com/image");

            postCreated.Url.ShouldBe("https://localtest.laughandgroan.com/image");
            postCreated.UserId.ShouldBe(userId.ToString());
            postCreated.PostId.ShouldNotBeNullOrWhiteSpace();

            var postFound = await _service.GetPost(userId.ToString(), postCreated.PostId);

            postFound.PostId.ShouldBe(postCreated.PostId);
            postFound.Url.ShouldBe(postCreated.Url);
            postFound.UserId.ShouldBe(userId.ToString());
        }
    }
}
