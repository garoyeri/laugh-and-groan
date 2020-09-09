namespace LaughAndGroan.Actions.Tests
{
    using System.Threading.Tasks;
    using NUlid;
    using Shouldly;

    using static Testing;

    public class PostsTests
    {
        public Task SetUp()
        {
            return CreateTables();
        }

        public async Task CanCreatePostAndReadItBack()
        {
            var userId = Ulid.NewUlid();
            var postCreated = await Posts.CreatePost(userId.ToString(), "https://localtest.laughandgroan.com/image");

            postCreated.Url.ShouldBe("https://localtest.laughandgroan.com/image");
            postCreated.UserId.ShouldBe(userId.ToString());
            postCreated.PostId.ShouldNotBeNullOrWhiteSpace();

            var postFound = await Posts.GetPost(userId.ToString(), postCreated.PostId);

            postFound.PostId.ShouldBe(postCreated.PostId);
            postFound.Url.ShouldBe(postCreated.Url);
            postFound.UserId.ShouldBe(userId.ToString());
        }
    }
}
