import * as cdk from "@aws-cdk/core";
import * as acm from "@aws-cdk/aws-certificatemanager";
import * as cloudfront from "@aws-cdk/aws-cloudfront";
import { HttpHeaders } from "@cloudcomponents/cdk-lambda-at-edge-pattern";
import { StaticWebsite } from "@cloudcomponents/cdk-static-website";

export interface FrontendProps {
  certificate: acm.ICertificate;
  domainName: string;
}

export class Frontend extends cdk.Construct {
  constructor(scope: cdk.Construct, id: string, props: FrontendProps) {
    super(scope, id);

    const httpHeaders = new HttpHeaders(this, "HttpHeaders", {
      httpHeaders: {
        "Content-Security-Policy":
          "default-src 'none'; img-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; object-src 'none'; connect-src 'self'",
        "Strict-Transport-Security":
          "max-age=31536000; includeSubdomains; preload",
        "Referrer-Policy": "same-origin",
        "X-XSS-Protection": "1; mode=block",
        "X-Frame-Options": "DENY",
        "X-Content-Type-Options": "nosniff",
        "Cache-Control": "no-cache",
      },
    });

    const website = new StaticWebsite(this, "StaticWebsite", {
      bucketConfiguration: {
        source: "resource/laugh-and-groan-website",
        removalPolicy: cdk.RemovalPolicy.DESTROY,
      },
      aliasConfiguration: {
        domainName: props.domainName,
        names: [`www.${props.domainName}`, props.domainName],
        acmCertRef: props.certificate.certificateArn,
        securityPolicy: cloudfront.SecurityPolicyProtocol.TLS_V1_2_2019,
      },
    });

    website.addLambdaFunctionAssociation(httpHeaders);
  }
}
