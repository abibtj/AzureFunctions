using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.IO;

namespace AzureFunctions
{
    public static class BlobTriggerFunction
    {
        [FunctionName("BlobTriggerFunction")]
        public static void Run([BlobTrigger("commentsblob/{name}", Connection = "")] Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
        }
    }
}
