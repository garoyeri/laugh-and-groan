import * as cdk from "@aws-cdk/core";
import { MainHostedZone } from "./main_hosted_zone";

export class HostedZonesStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const rootDomainName = new cdk.CfnParameter(this, "RootDomainName", {
      type: "String",
      description: "Root Domain Name",
      default: "laughandgroan.com",
    });

    const main = new MainHostedZone(this, "MainHostedZone", rootDomainName.valueAsString);

    new cdk.CfnOutput(this, "Nameservers", {
      value: cdk.Fn.join(",", main.hostedZone.hostedZoneNameServers || [])
    })

    new cdk.CfnOutput(this, "ZoneId", {
      value: main.hostedZone.hostedZoneId
    })
  }
}
