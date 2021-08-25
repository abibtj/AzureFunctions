using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AzureFunctions
{
    public static class FunctionQueue
    {
        [FunctionName("FunctionQueue")]
        public static void Run(
            [QueueTrigger("CommentsQueue", Connection = "AzureWebJobsStorage")] CommentEntity commentEntity,
            [Blob("commentsblob/{CommentBy}.txt", FileAccess.Write, Connection = "AzureWebJobsStorage")] TextWriter writer,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {commentEntity}");

            writer.WriteLine($"Comment: {commentEntity.Comment} - Comment by: {commentEntity.CommentBy}");

            log.LogInformation("Retrieved a queue item");
        }
    }
}
