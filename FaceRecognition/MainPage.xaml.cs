using FaceRecognition.Models;
using Newtonsoft.Json;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FaceRecognition
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public const string FaceApiKey = "f544592356a24ca0a8a206f104160dc6";
        public const string FaceApiUrl = "https://wally-faceapitest.cognitiveservices.azure.com/face/v1.0";

        private string _faceId1;
        private string _faceId2;

        public MainPage()
        {
            InitializeComponent();
        }

        async void btnPick1_Clicked(object sender, System.EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                });
                if (file == null) return;
                imgSelected1.Source = ImageSource.FromStream(() => {
                    var stream = file.GetStream();
                    return stream;
                });
                ResponseModel face = await MakeAnalysisRequest(file.Path);

                lblTotalFace1.Text = "Usa lentes : " + face.faceAttributes.glasses;
                lblGender1.Text = "Gender : " + face.faceAttributes.gender;
                lblAge1.Text = "Edad : " + face.faceAttributes.age;

                _faceId1 = face.faceId;
            }
            catch (Exception ex)
            {
                string test = ex.Message;
            }
        }

        async void btnPick2_Clicked(object sender, System.EventArgs e)
        {
            await CrossMedia.Current.Initialize();
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync(new Plugin.Media.Abstractions.PickMediaOptions
                {
                    PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
                });
                if (file == null) return;
                imgSelected2.Source = ImageSource.FromStream(() => {
                    var stream = file.GetStream();
                    return stream;
                });
                ResponseModel face = await MakeAnalysisRequest(file.Path);

                lblTotalFace2.Text = "Usa lentes : " + face.faceAttributes.glasses;
                lblGender2.Text = "Gender : " + face.faceAttributes.gender;
                lblAge2.Text = "Edad : " + face.faceAttributes.age;

                _faceId2 = face.faceId;
            }
            catch (Exception ex)
            {
                string test = ex.Message;
            }
        }


        public async Task<ResponseModel> MakeAnalysisRequest(string imageFilePath)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FaceApiKey);

            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false" +
                "&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses," +
                "emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";

            string uri = FaceApiUrl + "/detect?" + requestParameters;
            HttpResponseMessage response;
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(uri, content);

                string contentString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    List<ResponseModel> faceDetails = JsonConvert.DeserializeObject<List<ResponseModel>>(contentString);
                    if (faceDetails.Count != 0)
                    {
                        return faceDetails[0];
                    }
                }
            }

            return new ResponseModel();
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

        public async Task<Similarity> VerifyTwoFaces(string faceId1, string faceId2)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", FaceApiKey);

            string uri = FaceApiUrl + "/verify";
            HttpResponseMessage response;

            var json = JsonConvert.SerializeObject(new
            {
                faceId1 = faceId1,
                faceId2 = faceId2
            });

            using (StringContent content = new StringContent(json))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                response = await client.PostAsync(uri, content);

                string contentString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Similarity similarity = JsonConvert.DeserializeObject<Similarity>(contentString);
                    return similarity;
                }
            }
            return new Similarity();
        }

        private async void btnCompare_Clicked(object sender, EventArgs e)
        {
            Similarity similarity = await VerifyTwoFaces(_faceId1, _faceId2);

            lblIdentical.Text = $"Es idéntico: {(similarity.IsIdentical ? "Si" : "No")}";
            lblConfidence.Text = $"Certeza: {(similarity.Confidence * 100)}%";
        }

        private void BtnNext_Clicked(object sender, EventArgs e)
        {
            App.Current.MainPage = new OCRPage();
        }
    }
}
