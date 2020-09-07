using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Shouldly;

using LaughAndGroan.Actions;

namespace LaughAndGroan.Actions.Tests
{
    public class FunctionTests
    {
        public void TestToUpperFunction()
        {
            // Invoke the lambda function and confirm the response is a proper greeting
            var function = new Function();
            var context = new TestLambdaContext();
            var response = function.FunctionHandler(new APIGatewayHttpApiV2ProxyRequest {
                PathParameters = new Dictionary<string, string> {
                    { "name", "Garo Yeriazarian" }
                }
            }, context);

            response.Body.ShouldBe("Hello Garo Yeriazarian!");
            response.Headers["Content-Type"].ShouldBe("text/plain");
            response.StatusCode.ShouldBe(200);
        }
    }
}
