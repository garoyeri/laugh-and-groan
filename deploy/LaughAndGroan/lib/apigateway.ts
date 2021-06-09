import * as cdk from "@aws-cdk/core";
import * as api from "@aws-cdk/aws-apigatewayv2";
import * as api_int from "@aws-cdk/aws-apigatewayv2-integrations";
import * as api_auth from "@aws-cdk/aws-apigatewayv2-authorizers";
import * as acm from "@aws-cdk/aws-certificatemanager";
import * as route53 from "@aws-cdk/aws-route53";
import { Lambdas } from "./lambdas";

export interface ApiGatewayProps {
  domainName: string;
  lambdas: Lambdas;
  certificate: acm.ICertificate;
  authIssuer?: string;
  authClientId?: string;
  hostedZone: route53.IHostedZone;
}

export class ApiGateway extends cdk.Construct {
  constructor(scope: cdk.Construct, id: string, props: ApiGatewayProps) {
    super(scope, id);

    const gatewayDomainName = new api.DomainName(this, "ApiGatewayDomainName", {
      domainName: `api.${props.domainName}`,
      certificate: props.certificate,
    });

    const gateway = new api.HttpApi(this, "ApiGateway", {
      corsPreflight: {
        allowHeaders: ["Content-Type", "X-Amz-Date", "Authorization", "X-Api-Key", "X-Amz-Security-Token", "X-Amz-User-Agent"],
        allowMethods: [
          api.CorsHttpMethod.GET,
          api.CorsHttpMethod.HEAD,
          api.CorsHttpMethod.POST,
          api.CorsHttpMethod.PUT,
          api.CorsHttpMethod.DELETE,
          api.CorsHttpMethod.OPTIONS,
        ],
        allowOrigins: [`https://${props.domainName}`, `http://localhost:3000`, `https://localhost:5001`],
        maxAge: cdk.Duration.hours(1),
      },
      defaultDomainMapping: {
        domainName: gatewayDomainName,
      },
    });

    new route53.ARecord(this, "ApiGatewayDomainNameAlias", {
      zone: props.hostedZone,
      recordName: `api.${props.domainName}`,
      target: route53.RecordTarget.fromAlias({
        bind: (record) => {
          return {
            dnsName: gatewayDomainName.regionalDomainName,
            hostedZoneId: gatewayDomainName.regionalHostedZoneId,
          };
        },
      }),
    });

    let jwtAuthorizer: api.IHttpRouteAuthorizer = new api.HttpNoneAuthorizer();
    if (props.authIssuer && props.authClientId) {
      jwtAuthorizer = new api_auth.HttpJwtAuthorizer({
        jwtAudience: [props.authClientId],
        jwtIssuer: props.authIssuer
      });
    }
    const noneAuthorizer = new api.HttpNoneAuthorizer();

    // Proxy for ASP.NET Core
    gateway.addRoutes({
      path: "/api/{proxy+}",
      methods: [api.HttpMethod.ANY],
      integration: new api_int.LambdaProxyIntegration({
        handler: props.lambdas.aspnetLambda,
      }),
      authorizer: jwtAuthorizer || noneAuthorizer
    });

    // Swagger route
    gateway.addRoutes({
      path: "/swagger/{proxy+}",
      methods: [api.HttpMethod.ANY],
      integration: new api_int.LambdaProxyIntegration({
        handler: props.lambdas.aspnetLambda,
      }),
      authorizer: noneAuthorizer
    });

    // Health check route
    gateway.addRoutes({
      path: "/health",
      methods: [api.HttpMethod.ANY],
      integration: new api_int.LambdaProxyIntegration({
        handler: props.lambdas.aspnetLambda,
      }),
      authorizer: noneAuthorizer
    });

    new cdk.CfnOutput(this, "ApiUrl", {
      value: gateway.url ?? "",
      description: "URL to the default stage of the API",
    });
  }
}
