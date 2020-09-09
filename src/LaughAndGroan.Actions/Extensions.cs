namespace LaughAndGroan.Actions
{
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Text;
    using Amazon.Lambda.APIGatewayEvents;
    using Amazon.Lambda.Core;

    public static class Extensions
    {
        public static string SerializeObject<T>(this ILambdaSerializer serializer, T source)
        {
            using var memory = new MemoryStream();
            serializer.Serialize(source, memory);
            memory.Seek(0L, SeekOrigin.Begin);
            using var reader = new StreamReader(memory, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        public static T DeserializeObject<T>(this ILambdaSerializer serializer, string source)
        {
            using var memory = new MemoryStream(Encoding.UTF8.GetBytes(source));
            return serializer.Deserialize<T>(memory);
        }

        public static JwtSecurityToken GetAuthorization(this APIGatewayHttpApiV2ProxyRequest request)
        {
            if (request.Headers.TryGetValue("Authorization", out var authorization))
            {
                return new JwtSecurityToken(authorization);
            }
            return null;
        }
    }
}
