using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace WebAdvert.Web.Services
{
    public class S3FileUploader : IFileUploader
    {
        private readonly IConfiguration _configuration;

        public S3FileUploader(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> UploadFileAsync(string fileName, Stream storageStream)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentException(message: "File name must be specified.");
            var bucketName = _configuration.GetValue<string>(key: "ImageBucket");
            using (var client = new AmazonS3Client())
            {
                if(storageStream.Length > 0)
                    if (storageStream.CanSeek)
                        storageStream.Seek(offset:0, SeekOrigin.Begin);

                var request = new PutObjectRequest
                {
                    AutoCloseStream = true,
                    BucketName = bucketName,
                    InputStream = storageStream, //conteudo do arquivo
                    Key = fileName
                };

                //este metodo só funciona com arquvos pequenos
                var response = await client.PutObjectAsync(request).ConfigureAwait(continueOnCapturedContext: false);
                return response.HttpStatusCode == HttpStatusCode.OK;
                    
            }
        }
    }
}
