namespace LaughAndGroan.Actions.Tests
{
    using System;
    using System.Threading.Tasks;
    using Shouldly;

    using static Testing;

    public class UserServiceTests
    {
        public async Task CanCreateAndGetUser()
        {
            var userId = Guid.NewGuid().ToString();
            var user = await Users.Create(userId);

            user.UserId.ShouldBe(userId);
            user.UserName.ShouldNotBeNullOrWhiteSpace();

            var userFound = await Users.Get(user.UserName);
            userFound.UserId.ShouldBe(userId);
            userFound.UserName.ShouldBe(user.UserName);
        }
    }
}
