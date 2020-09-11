import * as cdk from "@aws-cdk/core";
import * as dynamodb from "@aws-cdk/aws-dynamodb";
import * as lambda from "@aws-cdk/aws-lambda";

export interface LambdasProps {
  tables: dynamodb.Table[];
}

export class Lambdas extends cdk.Construct {
  readonly postAuthTrigger: lambda.Function;
  readonly createPostLambda: lambda.Function;
  readonly getPostLambda: lambda.Function;
  readonly getPostsLambda: lambda.Function;
  readonly deletePostLambda: lambda.Function;

  constructor(scope: cdk.Construct, id: string, props: LambdasProps) {
    super(scope, id);

    this.postAuthTrigger = new lambda.Function(
      this,
      "UserPoolPostAuthenticationTrigger",
      {
        runtime: lambda.Runtime.DOTNET_CORE_3_1,
        code: lambda.Code.fromAsset("resource/LaughAndGroan.zip"),
        handler:
          "LaughAndGroan.Actions::LaughAndGroan.Actions.Users.PostAuthenticationHandler::CreateUserAfterAuthentication",
        logRetention: 30,
      }
    );

    this.createPostLambda = new lambda.Function(this, "CreatePostFunction", {
      runtime: lambda.Runtime.DOTNET_CORE_3_1,
      code: lambda.Code.fromAsset("resource/LaughAndGroan.zip"),
      handler:
        "LaughAndGroan.Actions::LaughAndGroan.Actions.Posts.PostsHandler::Create",
      logRetention: 30,
    });

    this.getPostLambda = new lambda.Function(this, "GetPostFunction", {
      runtime: lambda.Runtime.DOTNET_CORE_3_1,
      code: lambda.Code.fromAsset("resource/LaughAndGroan.zip"),
      handler:
        "LaughAndGroan.Actions::LaughAndGroan.Actions.Posts.PostsHandler::Get",
      logRetention: 30,
    });

    this.getPostsLambda = new lambda.Function(this, "GetPostsFunction", {
      runtime: lambda.Runtime.DOTNET_CORE_3_1,
      code: lambda.Code.fromAsset("resource/LaughAndGroan.zip"),
      handler:
        "LaughAndGroan.Actions::LaughAndGroan.Actions.Posts.PostsHandler::GetPosts",
      logRetention: 30,
    });

    this.deletePostLambda = new lambda.Function(this, "DeletePostFunction", {
      runtime: lambda.Runtime.DOTNET_CORE_3_1,
      code: lambda.Code.fromAsset("resource/LaughAndGroan.zip"),
      handler:
        "LaughAndGroan.Actions::LaughAndGroan.Actions.Posts.PostsHandler::Delete",
      logRetention: 30,
    });

    // give every lambda permission to use the dynamo tables
    [
      this.postAuthTrigger,
      this.deletePostLambda,
      this.getPostLambda,
      this.getPostsLambda,
      this.createPostLambda,
    ].forEach((l) => {
      props.tables.forEach((t) => {
        t.grantReadWriteData(l);
      });
    });
  }
}
