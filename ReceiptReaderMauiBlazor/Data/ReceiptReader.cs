using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace ReceiptReaderMauiBlazor.Data
{
    public class ReceiptReader
    {
        readonly string subscriptionKey = ""; // Enter the subscription key you get from the created Computer Vision resource.
        readonly string endpoint = ""; // Enter the endpoint you get from the created Computer Vision resource.

        private ReadResult _result { get; set; }

        public async Task<ReadResult> Read(string pathToFile)
        {
            if (string.IsNullOrEmpty(subscriptionKey))
                throw new Exception("No subscription key has been entered.");

            if (string.IsNullOrEmpty(endpoint))
                throw new Exception("Enter a valid endpoint.");

            ComputerVisionClient client = Authenticate();

            await ProcessFile(client, pathToFile);

            return _result;
        }

        private ComputerVisionClient Authenticate()
        {
            ComputerVisionClient client = new(new ApiKeyServiceClientCredentials(subscriptionKey))
            {
                Endpoint = endpoint
            };
            return client;
        }

        private async Task ProcessFile(ComputerVisionClient client, string pathToFile)
        {
            FileStream stream = File.OpenRead(pathToFile);
            ReadInStreamHeaders textHeaders = await client.ReadInStreamAsync(stream);

            Thread.Sleep(2000);

            string operationLocation = textHeaders.OperationLocation;
            string operationId = operationLocation[^36..];

            ReadOperationResult results;

            do
            {
                results = await client.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running || results.Status == OperationStatusCodes.NotStarted));

            _result = results.AnalyzeResult.ReadResults.First();
        }
    }
}
