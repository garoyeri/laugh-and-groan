namespace LaughAndGroan.Actions.Posts
{
    using System;

    public class PostApiRequest
    {
        public string Url { get; set; }
    }

    public class PostApiResponse
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string AuthorId { get; set; }
        public DateTimeOffset WhenPublished { get; set; }
    }

    public class GetPostsResponse
    {
        public PostApiResponse[] Data { get; set; }
    }
}
