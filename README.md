# Laugh and Groan: AWS Serverless Architecture

This project is an ongoing exploration of what it takes to setup a production serverless workload in AWS to do something "productive". The site will be a place to post links you find around the internet that either make you "laugh" or "groan" that you can share with people.

## Development Tools

1. PowerShell 7.1.x: [PowerShell/PowerShell: PowerShell for every system! (github.com)](https://github.com/PowerShell/PowerShell)
2. .NET SDK 5: [Download .NET 5.0 (Linux, macOS, and Windows) (microsoft.com)](https://dotnet.microsoft.com/download/dotnet/5.0)
3. AWS CLI: [AWS Command Line Interface (amazon.com)](https://aws.amazon.com/cli/)
4. Node JS LTS: https://nodejs.org/
5. Visual Studio Code: [Visual Studio Code - Code Editing. Redefined](https://code.visualstudio.com/)
6. Git: [Git (git-scm.com)](https://www.git-scm.com/)
7. Docker for Desktop: [Docker Desktop for Mac and Windows | Docker](https://www.docker.com/products/docker-desktop)

## First Time Build and Test

Run from PowerShell (`pwsh`)

```powershell
git clone https://github.com/garoyeri/laugh-and-groan.git
cd laugh-and-groan
./setup.ps1
invoke-psake SetupLocalDynamoDb
invoke-psake Restore
invoke-psake Test
```

Optional (but recommended), trust the ASP.NET Core development certificate so you can run HTTPS locally (more information: [Enforce HTTPS in ASP.NET Core | Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-5.0&tabs=visual-studio#trust-the-aspnet-core-https-development-certificate-on-windows-and-macos):

```powershell
dotnet dev-certs https trust
```

Open the project in Visual Studio Code or Visual Studio.

## Authentication

This example uses [Auth0](https://auth0.com/) for authentication (the free tier). You'll need to copy the settings from your own account into here to update it.

## Running the API

If you had rebooted since setting up the local DynamoDB, you should restart it:

```powershell
 invoke-psake startlocaldynamodb
 invoke-psake startapi
```

This will start a containerized DynamoDB that will be connected by the API automatically. Run the API using the default Visual Studio Code "Run without Debugging". Or you can do the same from Visual Studio. When the browser launches, it will show you an unhelpful main window, add `/swagger` to the URL to make this more interesting.

<https://locahost:5001/swagger>

Use the "Authorize" button to see the login screen, you can create a new account with your email or use your Google credentials to log in. Make sure you select "All scopes" and click "Authorize". Now all your API requests will pass a bearer token as well.

Try running the `GET /users/me` endpoint to see it in action.

## Run the Web Application

To start the web application, just run it using the build script:

```powershell
invoke-psake startweb
```

<http://localhost:3000>



