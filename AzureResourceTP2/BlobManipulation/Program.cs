using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

//https://docs.sixlabors.com/articles/imagesharp/resize.html
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace BlobManipulation
{
    class Program
    {
        private const string connectionString = "DefaultEndpointsProtocol=https;AccountName=gurostoragedemo;AccountKey=zHuaS9ILOACkcSbdEnr7bcsmLLPvBMOIjq5FtwcktrIUdOEtXGU02UGrtPKwGlPrqGX+mzRJEJI7+AStT22rTw==;EndpointSuffix=core.windows.net";
        private const string containerName = "initial";
        private const string containerName2 = "final";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Start : " + DateTime.Now.ToString());

            //Create service connection
            BlobServiceClient serviceClient = new BlobServiceClient(connectionString);

            //Create container 1 
            BlobContainerClient blobClient1 = serviceClient.GetBlobContainerClient(containerName);

            await CreateBlob(blobClient1, serviceClient, containerName);

            //Create container 2
            BlobContainerClient blobClient2 = serviceClient.GetBlobContainerClient(containerName2);

            await CreateBlob(blobClient2, serviceClient, containerName2);

            //File should be absent
            string path = @"C:\Users\gui44\OneDrive\Bureau\Meme\freebsd-devil.jpg";
            string filename = Path.GetFileName(path);

            //Upload a file to a Blob
            Console.WriteLine("File doesn't exist, uploading file ...");

            using (FileStream fileStream = File.OpenRead(path))
            {
                await blobClient1.UploadBlobAsync(filename, fileStream);
            }

            Console.WriteLine(("File uploaded : {0}", filename));


            //Download a file from Blob
            var blob = serviceClient.GetBlobContainerClient(containerName).GetBlockBlobClient(filename);

            //Since this is not inside of a using, it will need to be disposed.
            MemoryStream blobFile = new MemoryStream();

            await blob.DownloadToAsync(blobFile);

            using (Image image = Image.Load(blobFile))
            {
                int width = image.Width / 2;
                int height = image.Height / 2;
                image.Mutate(x => x.Resize(width, height));

                image.Save(blobFile, new JpegEncoder());
            }

            //Upload file to Blob
            //In here I'm using the second blob client that is connected on the "final" blob
            //I'm also using the modifyfile that is still in a stream format and the original name.
            await blobClient2.UploadBlobAsync(filename,blobFile);

            //Disposing the stream
            blobFile.Dispose();

            //Delete file from Original Blob
            await blob.DeleteAsync();
        }

        static async Task CreateBlob(BlobContainerClient blobClient, BlobServiceClient serviceClient, string containerName)
        {
            if (!blobClient.Exists())
            {
                blobClient = await serviceClient.CreateBlobContainerAsync(containerName);

                if (await blobClient.ExistsAsync())
                {
                    Console.WriteLine("Created container {0}", blobClient.Name);
                }
            }
            else
            {
                Console.WriteLine("Container {0} allready exist", blobClient.Name);
            }
        }
    }

}
