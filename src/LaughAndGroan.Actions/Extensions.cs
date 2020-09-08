namespace LaughAndGroan.Actions
{
    using System.IO;
    using System.Text;
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
    }
}
