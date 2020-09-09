namespace LaughAndGroan.Actions.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Amazon.DynamoDBv2;
    using Amazon.DynamoDBv2.Model;
    using Posts;
    using Shouldly;

    public static class Testing
    {
        static Testing()
        {
            Settings = new Settings
            {
                DynamoDbUrl = "http://localhost:8000/",
                TableNamePrefix = "LaughAndGroanLocalTest"
            };
            Client = new AmazonDynamoDBClient(new AmazonDynamoDBConfig {ServiceURL = Settings.DynamoDbUrl});
            Posts = new PostsService(Settings);
        }

        public static Settings Settings { get; }
        public static AmazonDynamoDBClient Client { get; }
        public static PostsService Posts { get; }

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
                        new KeySchemaElement("userId", KeyType.HASH)
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
                            IndexName = "UserNamesIndex",
                            KeySchema = new List<KeySchemaElement>
                            {
                                new KeySchemaElement("userName", KeyType.HASH)
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

            var postsCreateTableResponse = await Client.CreateTableAsync(postsTableName,
                new List<KeySchemaElement>
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
