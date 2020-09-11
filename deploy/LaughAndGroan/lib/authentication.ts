import * as cdk from "@aws-cdk/core";
import * as cognito from "@aws-cdk/aws-cognito";
import * as route53 from "@aws-cdk/aws-route53";
import * as targets from "@aws-cdk/aws-route53-targets";
import * as lambda from "@aws-cdk/aws-lambda";
import * as iam from "@aws-cdk/aws-iam";
import { AccountRecovery } from "@aws-cdk/aws-cognito";
import { ICertificate } from "@aws-cdk/aws-certificatemanager";

export interface AuthenticationProps {
  rootCertificate: ICertificate,
  rootHostedZone: route53.IHostedZone,
  postAuthTrigger: lambda.Function,
}

export class Authentication extends cdk.Construct {
  readonly userPool: cognito.UserPool;
  readonly userPoolClient: cognito.UserPoolClient;
  readonly userPoolDomain: cognito.UserPoolDomain;

  constructor(
    scope: cdk.Construct,
    id: string,
    props: AuthenticationProps,
  ) {
    super(scope, id);

    this.userPool = new cognito.UserPool(this, "UserPool", {
      accountRecovery: AccountRecovery.EMAIL_ONLY,
      autoVerify: {
        email: true,
        phone: false,
      },
      lambdaTriggers: {
        postAuthentication: props.postAuthTrigger
      },
      selfSignUpEnabled: true,
      signInAliases: {
        username: false,
        email: true,
        phone: false,
      },
    });

    this.userPoolClient = new cognito.UserPoolClient(this, "UserPoolClient", {
      userPool: this.userPool,
      oAuth: {
        callbackUrls: [`https://${props.rootHostedZone.zoneName}`]
      }
    });

    this.userPoolDomain = new cognito.UserPoolDomain(this, "UserPoolDomain", {
      userPool: this.userPool,
      customDomain: {
        domainName: `auth.${props.rootHostedZone.zoneName}`,
        certificate: props.rootCertificate,
      },
    });

    new route53.ARecord(this, "UserPoolDomainAlias", {
      zone: props.rootHostedZone,
      recordName: `auth.${props.rootHostedZone.zoneName}`,
      target: route53.RecordTarget.fromAlias(
        new targets.UserPoolDomainTarget(this.userPoolDomain)
      ),
    });
  }
}
