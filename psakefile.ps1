include "./build/general-helpers.ps1"

properties {
    $configuration = 'Release'
    $version = & dotnet minver -v e
    $owner = 'Garo Yeriazarian'
    $product = 'LaughAndGroan'
    $yearInitiated = '2020'
    $projectRootDirectory = "$(resolve-path .)"
    $publish = "$projectRootDirectory/deploy/LaughAndGroan/resource"
    $testResults = "$projectRootDirectory/test-results"
}

task default -depends Test
task CI -depends Clean, Restore, Test, Publish -description "Continuous Integration process"
task Rebuild -depends Clean, Compile -description "Rebuild the code and database, no testing"

task Info -description "Display runtime information" {
    exec {
        dotnet --info
        docker version
        write-host "node: $(& node --version)"
        write-host "npm: $(& npm --version)"
        write-host "Version: $version"
    }
}

task SetupLocalDynamoDb -depends Info -description "Setup local DynamoDb for testing" {
    exec { docker compose -p laugh-and-groan-local -f local-dynamodb.docker-compose.yml up -d } -workingDirectory test
}

task TearDownLocalDynamoDb -depends Info -description "Tear down local DynamoDb for testing" {
    exec { docker compose -p laugh-and-groan-local -f local-dynamodb.docker-compose.yml down } -workingDirectory test
}

task StartLocalDynamoDb -depends Info -description "Start local DynamoDb for testing" {
    exec { docker compose -p laugh-and-groan-local -f local-dynamodb.docker-compose.yml start } -workingDirectory test
}

task StopLocalDynamoDb -depends Info -description "Stop local DynamoDb for testing" {
    exec { docker compose -p laugh-and-groan-local -f local-dynamodb.docker-compose.yml stop } -workingDirectory test
}

task Test -depends Compile -description "Run unit tests" {
    get-childitem . test/*.Tests -directory | foreach-object {
        exec { dotnet fixie --configuration $configuration --no-build } -workingDirectory $_.fullname
    }
}

task Restore -depends Info -description "Restore all packages for build" {
    exec { dotnet restore }
    exec { npm install } -workingDirectory src/laugh-and-groan-website
    exec { npm install } -workingDirectory deploy/Certificates
    exec { npm install } -workingDirectory deploy/HostedZones
    exec { npm install } -workingDirectory deploy/LaughAndGroan
}
  
task Compile -depends Info -description "Compile the solution" {
    exec { dotnet build --configuration $configuration --nologo -p:"Product=$($product)" -p:"Copyright=$(get-copyright)" -p:"Version=$($version)" }
    exec { npm run build } -workingDirectory src/laugh-and-groan-website
    exec { npm run build } -workingDirectory deploy/Certificates
    exec { npm run build } -workingDirectory deploy/HostedZones
    exec { npm run build } -workingDirectory deploy/LaughAndGroan
}

task Publish -depends Compile -description "Publish the primary projects for distribution" {
    remove-directory-silently $publish
    exec { dotnet lambda package $publish/LaughAndGroan.zip -pt image -c Release --msbuild-parameters -p:"Product=$($product)" -p:"Copyright=$(get-copyright)" -p:"Version=$($version)" -p:"PublishReadyToRun=true" -p:"TieredCompilation=false" -p:"TieredCompilationQuickJit=false" } -workingDirectory src/LaughAndGroan.Api
    exec {
        Copy-Item src/laugh-and-groan-website/build $publish/laugh-and-groan-website -Recurse
        Move-Item $publish/laugh-and-groan-website/config.prod.js $publish/laugh-and-groan-website/config.js -Force
    }
}

task Deploy -depends Publish -description "Deploy the solution to AWS" {
    exec { npm run cdk -- deploy --require-approval never } -workingDirectory deploy/LaughAndGroan
}

task DeployDns -depends Publish -description "Deploy the DNS stack" {
    exec { npm run cdk -- deploy --require-approval never } -workingDirectory deploy/HostedZones
}

task DeployCerts -depends Publish -description "Deploy the Certificates stack" {
    exec { npm run cdk -- deploy --require-approval never } -workingDirectory deploy/Certificates
}

task Clean -description "Clean out all the binary folders" {
    exec { dotnet clean --configuration $configuration /nologo }
    remove-directory-silently $publish
    remove-directory-silently $testResults
    remove-directory-silently deploy/Certificates/cdk.out
    remove-directory-silently deploy/HostedZones/cdk.out
    remove-directory-silently deploy/LaughAndGroan/cdk.out
}

task StartWeb -description "Run the web application" {
    exec { npm start } -workingDirectory src/laugh-and-groan-website
}

task StartApi -description "Run the web API" {
    exec { dotnet run } -workingDirectory src/LaughAndGroan.Api
}
  
task ? -alias help -description "Display help content and possible targets" {
    WriteDocumentation
}