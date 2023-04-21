using FakeItEasy;
using System.Threading;
using System.Threading.Tasks;
using Transactions.Application.Users;
using Transactions.Domain;
using Transactions.Domain.Users;
using Transactions.Infrastructure;
using Xunit;

namespace Transactions.Tests
{
    public class RegisterCommandHandlerTest
    {
        private RegisterCommand command;
        private RegisterCommandHandler handler;

        private readonly ITransactionsRepository fakeRepository;

        public RegisterCommandHandlerTest()
        {
            var request = new RegisterRequest()
            {
                Name = "John",
                Surname = "Doe",
                Email = "johndoe@mail.com",
                Password = "1234"
            };
            fakeRepository = A.Fake<ITransactionsRepository>();

            command = new RegisterCommand(request);
            handler = new RegisterCommandHandler(fakeRepository);
        }

        [Fact]
        public async Task RegisterCommandHandler_NotRegisteredMail_ThrowsException()
        {
            var ex = await Assert.ThrowsAsync<HttpException>(async () => await handler.Handle(command, CancellationToken.None));

            Assert.Equal(409, ex.StatusCode);
            Assert.Equal("Email already registered", ex.Message);
            A.CallTo(() => fakeRepository.GetUserByEmailAsync(A<string>._)).MustHaveHappenedOnceExactly();
        }
    }
}
