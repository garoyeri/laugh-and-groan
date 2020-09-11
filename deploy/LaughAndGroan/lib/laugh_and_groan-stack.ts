import * as cdk from "@aws-cdk/core";
import * as acm from "@aws-cdk/aws-certificatemanager";
import * as route53 from "@aws-cdk/aws-route53";
import { Authentication } from "./authentication";
import { Lambdas } from "./lambdas";
import { Database } from "./database";
import { Frontend } from "./frontend";
import { ApiGateway } from "./apigateway";

export class LaughAndGroanStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // const rootDomainName = new cdk.CfnParameter(this, "RootDomainName", {
    //   type: "String",
    //   description: "Root Domain Name",
    //   default: "laughandgroan.com",
    // });

    const hostedZoneId = new cdk.CfnParameter(this, "HostedZoneId", {
      type: "String",
      description: "Hosted Zone ID for the root hosted zone",
    });

    const rootCertificateArn = new cdk.CfnParameter(
      this,
      "RootCertificateArn",
      {
        type: "String",
        description: "ARN for the wildcard certificate to use",
      }
    );

    const tableNamePrefix = new cdk.CfnParameter(this, "TableNamePrefix", {
      type: "String",
      description: "Table name prefix for DynamoDB tables",
      default: "LaughAndGroan",
    });

    const hostedZone = route53.HostedZone.fromHostedZoneAttributes(
      this,
      "MainHostedZone",
      {
        hostedZoneId: hostedZoneId.valueAsString,
        zoneName: "laughandgroan.com", //rootDomainName.valueAsString,
      }
    );

    const cert = acm.Certificate.fromCertificateArn(
      this,
      "RootCertificate",
      rootCertificateArn.valueAsString
    );

    const database = new Database(this, "Database", {
      tableNamePrefix: tableNamePrefix.valueAsString,
    });
    const lambdas = new Lambdas(this, "Lambdas", {
      tables: [database.usersTable, database.postsTable],
    });
    const auth = new Authentication(this, "Authentication", {
      rootCertificate: cert,
      rootHostedZone: hostedZone,
      postAuthTrigger: lambdas.postAuthTrigger,
    });
    const frontend = new Frontend(this, "Frontend", {
      domainName: "laughandgroan.com", //rootDomainName.valueAsString,
      certificate: cert,
    });
    const api = new ApiGateway(this, "ApiGateway", {
      domainName: "laughandgroan.com",
      certificate: cert,
      lambdas: lambdas,
      authClientId: auth.userPoolClient.userPoolClientId,
      authIssuer: auth.userPool.userPoolProviderUrl,
    })
  }
}
