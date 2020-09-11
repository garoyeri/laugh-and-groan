import * as cdk from "@aws-cdk/core";
import * as api from "@aws-cdk/aws-apigatewayv2";
import * as acm from "@aws-cdk/aws-certificatemanager";
import * as route53 from "@aws-cdk/aws-route53";
import * as route53targets from "@aws-cdk/aws-route53-targets";
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
        allowHeaders: ["Authorization"],
        allowMethods: [
          api.HttpMethod.GET,
          api.HttpMethod.POST,
          api.HttpMethod.PUT,
          api.HttpMethod.DELETE,
        ],
        allowOrigins: [`https://${props.domainName}`],
        maxAge: cdk.Duration.minutes(1),
        allowCredentials: true,
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

    let routes: api.HttpRoute[] = [];

    // POST /posts
    routes = routes.concat(
      gateway.addRoutes({
        path: "/posts",
        methods: [api.HttpMethod.POST],
        integration: new api.LambdaProxyIntegration({
          handler: props.lambdas.createPostLambda,
        }),
      })
    );

    // GET /posts?from={postId}&by={authorId}
    routes = routes.concat(
      gateway.addRoutes({
        path: "/posts",
        methods: [api.HttpMethod.GET],
        integration: new api.LambdaProxyIntegration({
          handler: props.lambdas.getPostsLambda,
        }),
      })
    );

    // GET /posts/{postId}
    routes = routes.concat(
      gateway.addRoutes({
        path: "/posts/{postId}",
        methods: [api.HttpMethod.GET],
        integration: new api.LambdaProxyIntegration({
          handler: props.lambdas.getPostLambda,
        }),
      })
    );

    // DELETE /posts/{postId}
    routes = routes.concat(
      gateway.addRoutes({
        path: "/posts/{postId}",
        methods: [api.HttpMethod.DELETE],
        integration: new api.LambdaProxyIntegration({
          handler: props.lambdas.deletePostLambda,
        }),
      })
    );

    if (props.authIssuer && props.authClientId) {
      const authorizer = new api.CfnAuthorizer(this, "ApiGatewayAuthorizer", {
        name: "ApiGatewayAuthorizer",
        apiId: gateway.httpApiId,
        authorizerType: "JWT",
        identitySource: ["$request.header.Authorization"],
        jwtConfiguration: {
          audience: [props.authClientId],
          issuer: props.authIssuer,
        },
      });

      // https://dev.to/martzcodes/token-authorizers-with-apigatewayv2-tricks-apigwv1-doesn-t-want-you-to-know-41jn
      routes.forEach((route) => {
        const routeCfn = route.node.defaultChild as api.CfnRoute;
        routeCfn.authorizerId = authorizer.ref;
        routeCfn.authorizationType = "JWT";
      });
    }

    new cdk.CfnOutput(this, "ApiUrl", {
      value: gateway.url ?? "",
      description: "URL to the default stage of the API",
    });
  }
}
