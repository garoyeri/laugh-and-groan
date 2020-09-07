import * as cdk from "@aws-cdk/core";
import * as route53 from "@aws-cdk/aws-route53";

export class MainHostedZone extends cdk.Construct {
  readonly hostedZone: route53.HostedZone;

  constructor(scope: cdk.Construct, id: string, domainName: string) {
    super(scope, id);

    this.hostedZone = new route53.HostedZone(this, "MainHostedZone", {
      zoneName: "laughandgroan.com",
    });
  }
}
