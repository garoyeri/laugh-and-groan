import * as cdk from "@aws-cdk/core";
import * as api from "@aws-cdk/aws-apigatewayv2";
import * as acm from "@aws-cdk/aws-certificatemanager";
import { Lambdas } from "./lambdas";

export interface ApiGatewayProps {
  domainName: string;
  lambdas: Lambdas;
  certificate: acm.ICertificate;
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

    // POST /posts
    gateway.addRoutes({
      path: "/posts",
      methods: [api.HttpMethod.POST],
      integration: new api.LambdaProxyIntegration({
        handler: props.lambdas.createPostLambda,
      }),
    });

    // GET /posts?from={postId}&by={authorId}
    gateway.addRoutes({
      path: "/posts",
      methods: [api.HttpMethod.GET],
      integration: new api.LambdaProxyIntegration({
        handler: props.lambdas.getPostsLambda,
      }),
    });

    // GET /posts/{postId}
    gateway.addRoutes({
      path: "/posts/{postId}",
      methods: [api.HttpMethod.GET],
      integration: new api.LambdaProxyIntegration({
        handler: props.lambdas.getPostLambda,
      }),
    });

    // DELETE /posts/{postId}
    gateway.addRoutes({
      path: "/posts/{postId}",
      methods: [api.HttpMethod.DELETE],
      integration: new api.LambdaProxyIntegration({
        handler: props.lambdas.deletePostLambda,
      }),
    });

    new cdk.CfnOutput(this, "ApiUrl", {
      value: gateway.url ?? "",
      description: "URL to the default stage of the API",
    });
  }
}
