using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Text;
using Xunit;

namespace AzureFunctions.Tests
{
    public class CreateCommentTests
    {
        [Fact]
        public void CreateComment_BadRequest()
        {
            // Arrange

            var context = new DefaultHttpContext();
            var logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");

            // Act

            var response = CommentFunction.CreateComment(context.Request, null, null, logger);// WatchPortalFunction.WatchInfo.Run(request, logger);
            response.Wait();

            //Assert 

            // Check that the response is a "Bad Request" response
            Assert.IsAssignableFrom<BadRequestObjectResult>(response.Result);
        }

        [Fact]
        public void CreateComment_Success()
        {
            // Arrange

            var comment = new CommentEntity
            {
                Comment = "Test comment",
                CommentBy = "Abeeb"
            };

            var stringComment = JsonConvert.SerializeObject(comment);

            var queryStringValue = "abc";

            var context = new DefaultHttpContext();
            context.Request.Query = new QueryCollection (
                    new Dictionary<string, StringValues>()
                    {
                        { "model", queryStringValue }
                    });

            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(stringComment));

            var logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");

            // Act

            var response = CommentFunction.CreateComment(context.Request, new AsyncCollector<CommentEntity>(), new AsyncCollector<CommentEntity>(), logger);// WatchPortalFunction.WatchInfo.Run(request, logger);
            response.Wait();

            // Assert

            // Check that the response is an "OK" response
            Assert.IsAssignableFrom<OkObjectResult>(response.Result);
        }
    }

    public class AsyncCollector<T> : IAsyncCollector<T>
    {
        public readonly List<T> Items = new();

        public Task AddAsync(T item, CancellationToken cancellationToken = default)
        {

            Items.Add(item);

            return Task.FromResult(true);
        }

        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }
    }
}
