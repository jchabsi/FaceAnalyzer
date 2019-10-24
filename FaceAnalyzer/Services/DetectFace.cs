using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;

namespace FaceAnalyzer.Services
{
    public class DetectFace
    {        
        // Add your Azure Computer Vision subscription key and endpoint to your appsettings.json file
        private string SubscriptionKey { get; set; }
        private string FaceEndpoint { get; set; }
        private string LocalImagePath { get; set; }

        private static readonly FaceAttributeType[] faceAttributes =
            { FaceAttributeType.Age, FaceAttributeType.Gender, FaceAttributeType.FacialHair,
              FaceAttributeType.Glasses, FaceAttributeType.Smile, FaceAttributeType.Makeup, FaceAttributeType.Hair,
              FaceAttributeType.Accessories, FaceAttributeType.Blur, FaceAttributeType.Emotion, FaceAttributeType.Exposure};

        public DetectFace(string image, IConfiguration configuration)
        {
            LocalImagePath = image;
            SubscriptionKey = configuration.GetValue<string>("subscriptionKey");
            FaceEndpoint = configuration.GetValue<string>("faceEndpoint");
        }

        public async Task<IList<DetectedFace>> Run()
        {
            FaceClient faceClient = new FaceClient(
                new ApiKeyServiceClientCredentials(SubscriptionKey),
                new System.Net.Http.DelegatingHandler[] { });

            faceClient.Endpoint = FaceEndpoint;
            var detectionTask = DetectLocalAsync(faceClient, LocalImagePath);

            return await detectionTask;                       
        }        

        // Detect faces in a local image
        private static async Task<IList<DetectedFace>> DetectLocalAsync(FaceClient faceClient, string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                throw new System.ArgumentException("Unable to open or read the image");                
            }

            IList<DetectedFace> faceList = null;

            try
            {
                using Stream imageStream = File.OpenRead(imagePath);
                faceList = await faceClient.Face.DetectWithStreamAsync(imageStream, true, false, faceAttributes);
            }
            catch (APIErrorException e)
            {
                Console.WriteLine(imagePath + ": " + e.Message);
            }
            return faceList;
        }        
    }
}
