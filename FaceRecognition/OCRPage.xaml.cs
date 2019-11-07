using Newtonsoft.Json;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Rest;
using Plugin.Media.Abstractions;
using System.Threading;

namespace FaceRecognition
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OCRPage : ContentPage
    {
        private string KEY = "399dd41ab36047fdbecdb09d65559dc8";
        //private string URI = "https://wally-ocrapitest.cognitiveservices.azure.com/vision/v2.0/analyze";
        private string URI = "https://wally-ocrapitest.cognitiveservices.azure.com/";

        private const TextRecognitionMode textRecognitionMode = TextRecognitionMode.Printed;
        private const int numberOfCharsInOperationId = 36;
        public OCRPage()
        {
            InitializeComponent();
        }

        private async Task MakeAnalysisRequest(string imageFilePath)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", KEY);

                // Request parameters. A third optional parameter is "details".
                // The Analyze Image method returns information about the following
                // visual features:
                // Categories:  categorizes image content according to a
                //              taxonomy defined in documentation.
                // Description: describes the image content with a complete
                //              sentence in supported languages.
                // Color:       determines the accent color, dominant color, 
                //              and whether an image is black & white.
                string requestParameters = "visualFeatures=Categories,Description,Color";

                // Assemble the URI for the REST API method.
                string uri = URI + "?" + requestParameters;

                HttpResponseMessage response;

                // Read the contents of the specified local image
                // into a byte array.
                byte[] byteData = GetImageAsByteArray(imageFilePath);

                // Add the byte array as an octet stream to the request body.
                using (ByteArrayContent content = new ByteArrayContent(byteData))
                {
                    // This example uses the "application/octet-stream" content type.
                    // The other content types you can use are "application/json"
                    // and "multipart/form-data".
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    // Asynchronously call the REST API method.
                    response = await client.PostAsync(uri, content);
                }

                // Asynchronously get the JSON response.
                string contentString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    OcrResponse ocrResponse = JsonConvert.DeserializeObject<OcrResponse>(contentString);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message);
            }
        }

        public byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (FileStream fileStream =
                new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            {
                BinaryReader binaryReader = new BinaryReader(fileStream);
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }

        public async Task MakeOCRAnalysis(MediaFile file)
        {
            using (var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(KEY),
                new DelegatingHandler[] { }) { Endpoint = URI })
            {
                BatchReadFileInStreamHeaders textHeaders =
                    await client.BatchReadFileInStreamAsync(file.GetStream(),CancellationToken.None);

                await GetTextAsync(client, textHeaders.OperationLocation);
            }
        }

        private async Task GetTextAsync(ComputerVisionClient client, string operationLocation)
        {
            // Retrieve the URI where the recognized text will be
            // stored from the Operation-Location header
            string operationId = operationLocation.Substring(
                operationLocation.Length - numberOfCharsInOperationId);

            ReadOperationResult result = await client.GetReadOperationResultAsync(operationId);

            // Wait for the operation to complete
            int i = 0;
            int maxRetries = 10;
            while ((result.Status == TextOperationStatusCodes.Running ||
                    result.Status == TextOperationStatusCodes.NotStarted) && i++ < maxRetries)
            {
                await Task.Delay(1000);

                result = await client.GetReadOperationResultAsync(operationId);
            }

            var recResults = result.RecognitionResults;
            string text = string.Empty;
            int count = 0;
            foreach (TextRecognitionResult recResult in recResults)
            {
                foreach (Line line in recResult.Lines)
                {
                    count += 1;
                    text += $"Línea {count}: {line.Text}    ,";
                }
            }
            lblG1.Text = text;
        }

        private async void BtnPick3_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                });
                if (file == null) return;
                imgSelected3.Source = ImageSource.FromStream(() => {
                    var stream = file.GetStream();
                    return stream;
                });

                //await MakeAnalysisRequest(file.Path);
                await MakeOCRAnalysis(file);
            }
            catch (Exception ex)
            {
                string test = ex.Message;
            }
        }
    }
}