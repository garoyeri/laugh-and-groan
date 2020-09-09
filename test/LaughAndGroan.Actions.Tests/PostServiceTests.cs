namespace LaughAndGroan.Actions.Tests
{
    using System;
    using System.Threading.Tasks;
    using Shouldly;

    using static Testing;

    public class PostServiceTests
    {
        public async Task CanCreatePostAndReadItBack()
        {
            var userId = Guid.NewGuid().ToString();
            var postCreated = await Posts.CreatePost(userId, "https://localtest.laughandgroan.com/image");

            postCreated.Url.ShouldBe("https://localtest.laughandgroan.com/image");
            postCreated.UserId.ShouldBe(userId);
            postCreated.PostId.ShouldNotBeNullOrWhiteSpace();

            var postFound = await Posts.GetPost(postCreated.PostId);

            postFound.PostId.ShouldBe(postCreated.PostId);
            postFound.Url.ShouldBe(postCreated.Url);
            postFound.UserId.ShouldBe(userId);
        }

        public async Task CanDeletePost()
        {
            var userId = Guid.NewGuid().ToString();
            var postCreated = await Posts.CreatePost(userId, "https://localtest.laughandgroan.com/image");

            var postFound = await Posts.GetPost(postCreated.PostId);
            postFound.ShouldNotBeNull();

            (await Posts.DeletePost(userId + "123", postFound.PostId)).ShouldBeFalse();
            (await Posts.DeletePost(userId, postFound.PostId)).ShouldBeTrue();

            var postFoundAfterDelete = await Posts.GetPost(postCreated.PostId);
            postFoundAfterDelete.ShouldBeNull();
        }
    }
}
