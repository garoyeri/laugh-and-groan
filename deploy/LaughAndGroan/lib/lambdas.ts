import * as cdk from "@aws-cdk/core";
import * as dynamodb from "@aws-cdk/aws-dynamodb";
import * as lambda from "@aws-cdk/aws-lambda";
import { IGrantable } from "@aws-cdk/aws-iam";
import * as path from "path";

export interface LambdasProps {
  tables: dynamodb.Table[];
}

export class Lambdas extends cdk.Construct {
  readonly aspnetLambda: lambda.Function;

  constructor(scope: cdk.Construct, id: string, props: LambdasProps) {
    super(scope, id);

    this.aspnetLambda = new lambda.DockerImageFunction(this, "AspNetLambdaFunction", {
      code: lambda.DockerImageCode.fromImageAsset(path.join(__dirname, "../../../src/LaughAndGroan.Api")),
      memorySize: 512,
      logRetention: 30,
      timeout: cdk.Duration.seconds(30),
    });   
    
    // give every lambda permission to use the dynamo tables
    [
      this.aspnetLambda
    ].forEach((l: IGrantable) => {
      props.tables.forEach((t) => {
        t.grantFullAccess(l);
      });
    });
  }
}
