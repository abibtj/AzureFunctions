using Microsoft.Azure.Cosmos.Table;
//using Microsoft.WindowsAzure.Storage.Table;

namespace AzureFunctions
{
    public class CommentEntity : TableEntity
    {
        public string Comment { get; set; }
        public string CommentBy { get; set; }
    }
}
