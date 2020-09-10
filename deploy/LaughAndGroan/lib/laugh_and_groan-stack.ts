import * as cdk from "@aws-cdk/core";
import * as acm from "@aws-cdk/aws-certificatemanager";
import * as route53 from "@aws-cdk/aws-route53";
import { Authentication } from "./authentication";

export class LaughAndGroanStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const rootDomainName = new cdk.CfnParameter(this, "RootDomainName", {
      type: "String",
      description: "Root Domain Name",
      default: "laughandgroan.com",
    });

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

    const hostedZone = route53.HostedZone.fromHostedZoneAttributes(
      this,
      "MainHostedZone",
      {
        hostedZoneId: hostedZoneId.valueAsString,
        zoneName: rootDomainName.valueAsString,
      }
    );

    const cert = acm.Certificate.fromCertificateArn(
      this,
      "RootCertificate",
      rootCertificateArn.valueAsString
    );

    const auth = new Authentication(this, "Authentication", cert, hostedZone);
  }
}
