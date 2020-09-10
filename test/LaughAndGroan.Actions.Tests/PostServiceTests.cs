namespace LaughAndGroan.Actions.Tests
{
    using System;
    using System.Linq;
    using System.Threading;
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
            var user1 = await Users.Create(userId1);
            var user2 = await Users.Create(userId2);

            const int count = 50;
            var links1 = Enumerable.Range(1, count).Select(n => $"https://localtest.laughandgroan.com/image/{n}").ToArray();
            var links2 = Enumerable.Range(101, count).Select(n => $"https://localtest.laughandgroan.com/image/{n}").ToArray();
            
            var now = new DateTimeOffset(2020, 08, 01, 12, 0, 0, TimeSpan.Zero);
            for (var i = 0; i < count; i++)
            {
                await Posts.CreatePost(userId1, links1[i], now.AddMinutes(i));
                await Posts.CreatePost(userId2, links2[i], now.AddMinutes(i).AddSeconds(30));
            }

            // we should now have interspersed posts from userid1 and 2 in 30 second increments

            var allPosts = await Posts.GetPosts(null, new[] { user1.UserName, user2.UserName });
            allPosts[0].Url.ShouldBe("https://localtest.laughandgroan.com/image/150");
            allPosts[0].UserId.ShouldBe(userId2);
            allPosts[1].Url.ShouldBe("https://localtest.laughandgroan.com/image/50");
            allPosts[1].UserId.ShouldBe(userId1);

            var onlyUser2Posts = await Posts.GetPosts(null, new[] { user2.UserName });
            onlyUser2Posts[0].Url.ShouldBe("https://localtest.laughandgroan.com/image/150");
            onlyUser2Posts[0].UserId.ShouldBe(userId2);
            onlyUser2Posts[1].Url.ShouldBe("https://localtest.laughandgroan.com/image/149");
            onlyUser2Posts[1].UserId.ShouldBe(userId2);

            var onlySomePosts = await Posts.GetPosts(onlyUser2Posts[2].PostId, new[] { user2.UserName });
            onlySomePosts[0].Url.ShouldBe("https://localtest.laughandgroan.com/image/147");
            onlySomePosts[0].UserId.ShouldBe(userId2);
            onlySomePosts[1].Url.ShouldBe("https://localtest.laughandgroan.com/image/146");
            onlySomePosts[1].UserId.ShouldBe(userId2);
        }
    }
}
