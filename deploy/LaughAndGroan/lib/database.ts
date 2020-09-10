import * as cdk from "@aws-cdk/core";
import * as dynamodb from "@aws-cdk/aws-dynamodb";

export class Database extends cdk.Construct {
  readonly postsTable: dynamodb.Table;
  readonly usersTable: dynamodb.Table;

  constructor(scope: cdk.Construct, id: string, tableNamePrefix: string) {
    super(scope, id);

    this.usersTable = new dynamodb.Table(this, "UsersTable", {
      tableName: `${tableNamePrefix}Users`,
      partitionKey: { name: "userName", type: dynamodb.AttributeType.STRING }      ,
    });

    this.usersTable.addGlobalSecondaryIndex({
      indexName: "UserIdIndex",
      partitionKey: { name: "userId", type: dynamodb.AttributeType.STRING },
      projectionType: dynamodb.ProjectionType.ALL
    });


    this.postsTable = new dynamodb.Table(this, "PostsTable", {
      tableName: `${tableNamePrefix}Posts`,
      partitionKey: { name: "postId", type: dynamodb.AttributeType.STRING },
    });

    this.postsTable.addGlobalSecondaryIndex({
      indexName: "UserIdIndex",
      partitionKey: { name: "userId", type: dynamodb.AttributeType.STRING },
      sortKey: { name: "postId", type: dynamodb.AttributeType.STRING },
      projectionType: dynamodb.ProjectionType.ALL
    });

    this.postsTable.addGlobalSecondaryIndex({
      indexName: "ChronologicalPostsIndex",
      partitionKey: { name: "type", type: dynamodb.AttributeType.STRING },
      sortKey: { name: "postId", type: dynamodb.AttributeType.STRING },
      projectionType: dynamodb.ProjectionType.ALL
    });
  }
}
