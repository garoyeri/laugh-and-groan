## Website Demo

<https://laughandgroan.com>

* Login using Google and regular account
* Make a post

## Review Entry Points

* [Lambda Entry Point](../src/LaughAndGroan.Api/LambdaEntryPoint.cs)
* [Local Entry Point](../src/LaughAndGroan.Api/LocalEntryPoint.cs)

## Prove this runs locally

Start DynamoDB locally:

```powershell
invoke-psake StartLocalDynamoDb
```

* [DynamoDB Docker Compose](../test/local-dynamodb.docker-compose.yml)

Run the API:

```powershell
invoke-psake StartApi
```

Run the web interface:

```powershell
invoke-psake StartWeb
```

* Web: <http://localhost:3000>
  * Login and see the web page
* Api: <https://localhost:5001/swagger>
  * Login with the "Authorize" link
  * Call the `GET /users/me` API

## Explore Deployment

Rebuild and deploy everything:

```powershell
$Env:AWS_PROFILE="hs-garo"
invoke-psake deploy
```

* Look at `Dockerfile`: [link](../src/LaughAndGroan.Api/Dockerfile)
* Look at CDK
  * [API Gateway](../deploy/LaughAndGroan/lib/apigateway.ts)
  * [Lambdas](../deploy/LaughAndGroan/lib/lambdas.ts)
  * [Frontend](../deploy/LaughAndGroan/lib/frontend.ts)
* Look at AWS Account
  * [API Gateway](https://console.aws.amazon.com/apigateway/main/api-detail?api=1r9gap2of7&region=us-east-1)
  * [DynamoDB Tables](https://console.aws.amazon.com/dynamodb/home?region=us-east-1#tables:)
  * [CloudWatch Insights](https://console.aws.amazon.com/cloudwatch/home?region=us-east-1#logsV2:logs-insights)
    * Show saved query for LaughAndGroan requests
    * Show template query for Lambda timing
    * Show template query for overprovisioned memory
  * [Lambda Console](https://console.aws.amazon.com/lambda/home?region=us-east-1#/functions/LaughAndGroanStack-LambdasAspNetLambdaFunctionEF86-HCx42p3F7KK7?tab=image)
