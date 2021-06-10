namespace LaughAndGroan.Api.Features.Posts
{
    using System;
    using NUlid;

    public class PostApiRequest
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }

    public class PostApiResponse
    {
        public PostApiResponse()
        {
        }

        public PostApiResponse(PostData source)
        {
            Id = source.PostId;
            Title = source.Title;
            AuthorId = source.UserId;
            Url = source.Url;
            WhenPublished = Ulid.Parse(source.PostId).Time;
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string AuthorId { get; set; }
        public DateTimeOffset WhenPublished { get; set; }
    }

    public class GetPostsResponse
    {
        public PostApiResponse[] Data { get; set; }
    }
}
