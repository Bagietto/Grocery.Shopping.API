using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using NSubstitute;

namespace Grocery.Shopping.API.Tests.Helpers
{
    public static class MongoDbTestHelpers
    {
        public static IMongoCollection<T> CreateMockCollection<T>(List<T> data)
        {
            var mockCollection = Substitute.For<IMongoCollection<T>>();

            var mockCursor = Substitute.For<IAsyncCursor<T>>();

            // Setup cursor to return our list on the first call, and false on the second (EOF)
            mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(
                _ => Task.FromResult(true),
                _ => Task.FromResult(false)
            );
            mockCursor.Current.Returns(data);

            // Mock the interface method FindAsync that is called by FindFluent under the hood
            mockCollection.FindAsync<T>(
                Arg.Any<IClientSessionHandle>(),
                Arg.Any<FilterDefinition<T>>(),
                Arg.Any<FindOptions<T, T>>(),
                Arg.Any<CancellationToken>()
            ).Returns(Task.FromResult(mockCursor));

            // Also mock the non-session FindAsync just in case
            mockCollection.FindAsync<T>(
                Arg.Any<FilterDefinition<T>>(),
                Arg.Any<FindOptions<T, T>>(),
                Arg.Any<CancellationToken>()
            ).Returns(Task.FromResult(mockCursor));

            return mockCollection;
        }
    }
}
