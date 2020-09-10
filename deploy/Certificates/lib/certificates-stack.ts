import * as cdk from "@aws-cdk/core";
import * as acm from "@aws-cdk/aws-certificatemanager";
import * as route53 from "@aws-cdk/aws-route53";

export class CertificatesStack extends cdk.Stack {
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

    const hostedZone = route53.HostedZone.fromHostedZoneAttributes(
      this,
      "MainHostedZone",
      {
        hostedZoneId: hostedZoneId.valueAsString,
        zoneName: rootDomainName.valueAsString,
      }
    );

    const certificate = new acm.DnsValidatedCertificate(
      this,
      "RootWildcardCertificate",
      {
        domainName: rootDomainName.valueAsString,
        subjectAlternativeNames: ["*." + rootDomainName.valueAsString],
        hostedZone: hostedZone,
      }
    );

    new cdk.CfnOutput(this, "CertificateArn", {
      value: certificate.certificateArn,
      description: "Root certificate ARN"
    })
  }
}
