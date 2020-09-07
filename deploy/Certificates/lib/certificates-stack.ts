import * as cdk from "@aws-cdk/core";
import * as acm from "@aws-cdk/aws-certificatemanager";

export class CertificatesStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const rootDomainName = new cdk.CfnParameter(this, "RootDomainName", {
      type: "String",
      description: "Root Domain Name",
      default: "laughandgroan.com",
    });

    const certificate = new acm.Certificate(this, "RootCertificate", {
      domainName: rootDomainName.valueAsString,
      subjectAlternativeNames: ["*." + rootDomainName.valueAsString],
      validation: acm.CertificateValidation.fromDns(),
    });
  }
}
