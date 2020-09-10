import * as cdk from "@aws-cdk/core";
import * as cognito from "@aws-cdk/aws-cognito";
import * as route53 from "@aws-cdk/aws-route53";
import * as targets from "@aws-cdk/aws-route53-targets";
import * as lambda from "@aws-cdk/aws-lambda";
import * as iam from "@aws-cdk/aws-iam";
import { AccountRecovery } from "@aws-cdk/aws-cognito";
import { ICertificate } from "@aws-cdk/aws-certificatemanager";

export class Authentication extends cdk.Construct {
  readonly userPool: cognito.UserPool;
  readonly userPoolClient: cognito.UserPoolClient;
  readonly userPoolDomain: cognito.UserPoolDomain;

  constructor(
    scope: cdk.Construct,
    id: string,
    rootCertificate: ICertificate,
    rootHostedZone: route53.IHostedZone
  ) {
    super(scope, id);

    const postAuthTrigger = new lambda.Function(this, "UserPoolPostAuthenticationTrigger", {
      runtime: lambda.Runtime.DOTNET_CORE_3_1,
      code: lambda.Code.fromAsset("resource/LaughAndGroan.zip"),
      handler: "LaughAndGroan.Actions::LaughAndGroan.Actions.Users.PostAuthenticationHandler::CreateUserAfterAuthentication",
      logRetention: 30,
    });  

    this.userPool = new cognito.UserPool(this, "UserPool", {
      accountRecovery: AccountRecovery.EMAIL_ONLY,
      autoVerify: {
        email: true,
        phone: false,
      },
      lambdaTriggers: {
        postAuthentication: postAuthTrigger
      },
    });

    this.userPoolClient = new cognito.UserPoolClient(this, "UserPoolClient", {
      userPool: this.userPool,
      oAuth: {
        callbackUrls: [`https://${rootHostedZone.zoneName}`]
      }
    });

    this.userPoolDomain = new cognito.UserPoolDomain(this, "UserPoolDomain", {
      userPool: this.userPool,
      customDomain: {
        domainName: `auth.${rootHostedZone.zoneName}`,
        certificate: rootCertificate,
      },
    });

    new route53.ARecord(this, "UserPoolDomainAlias", {
      zone: rootHostedZone,
      target: route53.RecordTarget.fromAlias(
        new targets.UserPoolDomainTarget(this.userPoolDomain)
      ),
    });
  }
}
