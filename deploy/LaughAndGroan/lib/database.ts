import * as cdk from "@aws-cdk/core";
import * as dynamodb from "@aws-cdk/aws-dynamodb";

export interface DatabaseProps {
  tableNamePrefix: string
}

export class Database extends cdk.Construct {
  readonly postsTable: dynamodb.Table;
  readonly usersTable: dynamodb.Table;

  constructor(scope: cdk.Construct, id: string, props: DatabaseProps) {
    super(scope, id);

    this.usersTable = new dynamodb.Table(this, "UsersTable", {
      tableName: `${props.tableNamePrefix}Users`,
      partitionKey: { name: "userName", type: dynamodb.AttributeType.STRING },
      pointInTimeRecovery: true,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
    });

    this.usersTable.addGlobalSecondaryIndex({
      indexName: "UserIdIndex",
      partitionKey: { name: "userId", type: dynamodb.AttributeType.STRING },
      projectionType: dynamodb.ProjectionType.ALL,
    });

    this.postsTable = new dynamodb.Table(this, "PostsTable", {
      tableName: `${props.tableNamePrefix}Posts`,
      partitionKey: { name: "postId", type: dynamodb.AttributeType.STRING },
      pointInTimeRecovery: true,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
    });

    this.postsTable.addGlobalSecondaryIndex({
      indexName: "UserIdIndex",
      partitionKey: { name: "userId", type: dynamodb.AttributeType.STRING },
      sortKey: { name: "postId", type: dynamodb.AttributeType.STRING },
      projectionType: dynamodb.ProjectionType.ALL,
    });

    this.postsTable.addGlobalSecondaryIndex({
      indexName: "ChronologicalPostsIndex",
      partitionKey: { name: "type", type: dynamodb.AttributeType.STRING },
      sortKey: { name: "postId", type: dynamodb.AttributeType.STRING },
      projectionType: dynamodb.ProjectionType.ALL,
    });
  }
}
