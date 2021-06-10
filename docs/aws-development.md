AWS development tools for .NET

## Install AWS Lambda Tools and Templates

From the AWS Lambda .NET GitHub page: [aws/aws-lambda-dotnet: Libraries, samples and tools to help .NET Core developers develop AWS Lambda functions. (github.com)](https://github.com/aws/aws-lambda-dotnet)

```powershell
dotnet tool install -g Amazon.Lambda.Tools
dotnet tool install -g amazon.lambda.testtool-5.0
dotnet new -i "Amazon.Lambda.Templates::*"
```

## Running DynamoDB Locally for Testing

From the AWS DynamoDB website: [Deploying DynamoDB Locally on Your Computer - Amazon DynamoDB](https://docs.aws.amazon.com/amazondynamodb/latest/developerguide/DynamoDBLocal.DownloadingAndRunning.html)

## Lambda Docker .NET 5 Image

Lambda doesn't use a built-in runtime anymore for .NET 5, instead, you use the container image provided by AWS that has the Lambda runtime, .NET 5 runtime, and an entrypoint script that handles the heavy lifting. More on that here: [.NET 5 AWS Lambda Support with Container Images | AWS Developer Blog (amazon.com)](https://aws.amazon.com/blogs/developer/net-5-aws-lambda-support-with-container-images/).
