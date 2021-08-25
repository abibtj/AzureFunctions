using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
//using Microsoft.WindowsAzure.Storage.Table;

namespace AzureFunctions
{
    public static class CommentFunction
    {
        [FunctionName("CreateComment")]
        public static async Task<IActionResult> CreateComment(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/comment")] HttpRequest req,
            [Table("CommentsTable", Connection = "AzureWebJobsStorage")] IAsyncCollector<CommentEntity> commentsTable,
            [Queue("CommentsQueue", Connection = "AzureWebJobsStorage")] IAsyncCollector<CommentEntity> commentsQueue,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a POST request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<CommentEntity>(requestBody);

            if (data is null)
                return new BadRequestObjectResult("Invalid Body");

            data.PartitionKey = "comments";
            data.RowKey = Guid.NewGuid().ToString("n");

            await commentsTable.AddAsync(data);
            await commentsQueue.AddAsync(data);

            return new OkObjectResult("Successful");
        }

        [FunctionName("GetComments")]
        public static async Task<IActionResult> GetComments(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/comment")] HttpRequest req,
            [Table("CommentsTable", Connection = "AzureWebJobsStorage")] CloudTable commentsTable,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a GET request.");

           var query = new TableQuery<CommentEntity>();
           var segment = await commentsTable.ExecuteQuerySegmentedAsync(query,null);

            return new OkObjectResult(segment);
        }
    }
}
