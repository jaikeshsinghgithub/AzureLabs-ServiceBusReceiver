#r "Microsoft.ServiceBus"
#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Configuration;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

public static void Run(BrokeredMessage mySbMsg, TraceWriter log)
{
    log.Info($"C# ServiceBus topic trigger function processed message: {mySbMsg.Label}");

    var acct = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureWebJobsStorage"]);
    var client = acct.CreateCloudBlobClient();
    var sourceContainer = client.GetContainerReference("upload");
    var sourceThumbContainer = client.GetContainerReference("uploadthumb");
    var savedContainer = client.GetContainerReference("saved");
    var savedThumbContainer = client.GetContainerReference("savedthumb");
    sourceContainer.CreateIfNotExists();
    sourceContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
    sourceThumbContainer.CreateIfNotExists();
    sourceThumbContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
    savedContainer.CreateIfNotExists();
    savedContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
    savedThumbContainer.CreateIfNotExists();
    savedThumbContainer.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

    log.Info("Moving image to saved storage; deleting from upload container.");
    using(var stream = new MemoryStream()) {
        var sourceBlob = sourceContainer.GetBlockBlobReference(mySbMsg.Label);
        sourceBlob.DownloadToStream(stream);
        stream.Position = 0;
        var uploadBlob = savedContainer.GetBlockBlobReference(mySbMsg.Label);
        uploadBlob.Properties.ContentType = "image/jpg";
        uploadBlob.UploadFromStream(stream);
        sourceBlob.Delete();
    }
    
    log.Info("Moving thumbnail to saved storage; deleting from uploadthumb container.");
    using(var stream = new MemoryStream()) {
        var sourceThumb = sourceThumbContainer.GetBlockBlobReference(mySbMsg.Label);
        sourceThumb.DownloadToStream(stream);
        stream.Position = 0;
        var uploadedThumb = savedThumbContainer.GetBlockBlobReference(mySbMsg.Label);
        uploadedThumb.Properties.ContentType = "image/jpg";
        uploadedThumb.UploadFromStream(stream);
        sourceThumb.Delete();
    }
    
    log.Info("Function complete.");
    
}
