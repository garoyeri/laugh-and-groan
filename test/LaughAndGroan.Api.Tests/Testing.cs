namespace LaughAndGroan.Api.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.Model;
    using Amazon.Runtime;
    using LaughAndGroan.Api.Features.Posts;
    using LaughAndGroan.Api.Features.Users;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Shouldly;

    public static class Testing
    {
        static Testing()
        {
            Factory = new TestApplicationFactory();
            Configuration = Factory.Services.GetRequiredService<IConfiguration>();
            ScopeFactory = Factory.Services.GetRequiredService<IServiceScopeFactory>();

            Settings = Factory.Services.GetRequiredService<IOptions<Settings>>().Value;
            Client = Factory.Services.GetRequiredService<IAmazonDynamoDB>();
            Posts = Factory.Services.GetRequiredService<PostsService>();
            Users = Factory.Services.GetRequiredService<UsersService>();
        }

        public class TestApplicationFactory : WebApplicationFactory<Startup>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.ConfigureAppConfiguration((_, configBuilder) =>
                {
                    configBuilder.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        { "DynamoDB::ServiceURL", "http://localhost:8000" }
                    });
                });
            }
        }

        private static TestApplicationFactory Factory;
        private static IConfiguration Configuration;
        private static IServiceScopeFactory ScopeFactory;

        public static Settings Settings { get; }
        public static IAmazonDynamoDB Client { get; }
        public static PostsService Posts { get; }
        public static UsersService Users { get; }

        /// <summary>
        /// Create the tables in DynamoDB
        /// </summary>
        /// <returns></returns>
        public static async Task CreateTables()
        {
            // create the Users table
            var usersTableName = Settings.TableNamePrefix + "Users";
            if (await WaitForTable(usersTableName, retries: 1) != null)
            {
                await Client.DeleteTableAsync(usersTableName);
                (await WaitForTable(usersTableName)).ShouldBeNull();
            }

            var usersCreateTableResponse = await Client.CreateTableAsync(new CreateTableRequest
                {
                    TableName = usersTableName,
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("userName", KeyType.HASH)
                    },
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition("userId", ScalarAttributeType.S),
                        new AttributeDefinition("userName", ScalarAttributeType.S)
                    },
                    ProvisionedThroughput = new ProvisionedThroughput(10, 5),
                    GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                    {
                        new GlobalSecondaryIndex
                        {
                            IndexName = "UserIdIndex",
                            KeySchema = new List<KeySchemaElement>
                            {
                                new KeySchemaElement("userId", KeyType.HASH)
                            },
                            Projection = new Projection { ProjectionType = ProjectionType.ALL },
                            ProvisionedThroughput = new ProvisionedThroughput(10, 5)
                        }
                    }
                }
            );
            (await WaitForTable(usersTableName, usersCreateTableResponse.TableDescription.TableStatus)).ShouldBe(TableStatus.ACTIVE);


            // create the Posts table
            var postsTableName = Settings.TableNamePrefix + "Posts";
            if (await WaitForTable(postsTableName, retries: 1) != null)
            {
                await Client.DeleteTableAsync(postsTableName);
                (await WaitForTable(postsTableName)).ShouldBeNull();
            }

            var postsCreateTableResponse = await Client.CreateTableAsync(new CreateTableRequest
                {
                    TableName = postsTableName,
                    KeySchema = new List<KeySchemaElement>
                    {
                        new KeySchemaElement("postId", KeyType.HASH)
                    },
                    AttributeDefinitions = new List<AttributeDefinition>
                    {
                        new AttributeDefinition("userId", ScalarAttributeType.S),
                        new AttributeDefinition("postId", ScalarAttributeType.S),
                        new AttributeDefinition("type", ScalarAttributeType.S),
                    },
                    GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>
                    {
                        new GlobalSecondaryIndex
                        {
                            IndexName = "UserIdIndex",
                            KeySchema = new List<KeySchemaElement>
                            {
                                new KeySchemaElement("userId", KeyType.HASH),
                                new KeySchemaElement("postId", KeyType.RANGE)
                            },
                            Projection = new Projection { ProjectionType = ProjectionType.ALL },
                            ProvisionedThroughput = new ProvisionedThroughput(10, 5)
                        },
                        new GlobalSecondaryIndex
                        {
                            IndexName = "ChronologicalPostsIndex",
                            KeySchema = new List<KeySchemaElement>
                            {
                                new KeySchemaElement("type", KeyType.HASH),
                                new KeySchemaElement("postId", KeyType.RANGE)
                            },
                            Projection = new Projection
                            {
                                ProjectionType = ProjectionType.ALL,
                            },
                            ProvisionedThroughput = new ProvisionedThroughput(10, 5)
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput(10, 5)
                }
            );
            (await WaitForTable(postsTableName, postsCreateTableResponse.TableDescription.TableStatus)).ShouldBe(TableStatus.ACTIVE);
        }

        /// <summary>
        /// Wait for the table to be created.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="initialStatus"></param>
        /// <param name="retries"></param>
        /// <param name="delay"></param>
        /// <returns></returns>
        public static async Task<TableStatus> WaitForTable(string tableName, TableStatus initialStatus = null,
            int retries = 12, TimeSpan delay = default)
        {
            delay = delay == TimeSpan.Zero ? TimeSpan.FromSeconds(5) : delay;
            retries = retries <= 1 ? 1 : retries;

            try
            {
                var status = initialStatus;
                for (var i = 0; i < retries && status != TableStatus.ACTIVE; i++)
                {
                    await Task.Delay(delay);
                    var response = await Client.DescribeTableAsync(tableName);
                    status = response.Table.TableStatus;
                }

                return status;
            }
            catch (ResourceNotFoundException)
            {
                return null;
            }
        }
    }
}
