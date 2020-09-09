namespace LaughAndGroan.Actions.Tests
{
    using System;
    using System.Linq;
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

        public async Task CanGetPosts()
        {
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();
            var links1 = Enumerable.Range(1, 50).Select(n => $"https://localtest.laughandgroan.com/image/{n}").ToArray();
            var links2 = Enumerable.Range(100, 50).Select(n => $"https://localtest.laughandgroan.com/image/{n}").ToArray();
            
            var now = DateTimeOffset.UtcNow;
            for (var i = 0; i < links1.Length; i++)
            {
                await Posts.CreatePost(userId1, links1[i], now.AddMinutes(-1 * i));
            }

            for (var i = 0; i < links2.Length; i++)
            {
                await Posts.CreatePost(userId2, links2[i], now.AddMinutes(-1 * i).AddSeconds(-30));
            }

            // we should now have interspersed posts from userid1 and 2 in 30 second increments

            var allPosts = await Posts.GetPosts(null, new[] { userId1, userId2 });
            allPosts[0].Url.ShouldBe("https://localtest.laughandgroan.com/image/1");
            allPosts[0].UserId.ShouldBe(userId1);

            allPosts[1].Url.ShouldBe("https://localtest.laughandgroan.com/image/100");
            allPosts[1].UserId.ShouldBe(userId2);
        }
    }
}
