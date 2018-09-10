using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace XUnitTestProject1
{
    public class xUnitTest1 : IClassFixture<LaunchSettingsFixture>
    {
        /// <summary>
        /// LaunchSettingsFixture reads run settings from Properties\launchSettings.json and sets them as environment variables
        /// </summary>
        LaunchSettingsFixture fixture;

        private static string SubscriptionKey;
        private static string AppId;
        private static string Endpoint;

        public xUnitTest1(LaunchSettingsFixture fixture)
        {
            this.fixture = fixture; // Sets environment variables from the Properties\launchsettings.json file
            // Set configuration values for all tests
            SubscriptionKey = Environment.GetEnvironmentVariable("subscriptionKey");
            AppId = Environment.GetEnvironmentVariable("appId"); ;
            Endpoint = Environment.GetEnvironmentVariable("endpoint");
        }

        [Fact]
        public async Task SingleUtteranceTest()
        {
            // Create client with SuscriptionKey and AzureRegion
            var client = new LUISRuntimeClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
            {
                Endpoint = Endpoint
            };

            // Predict
            LuisResult result = await client.Prediction.ResolveAsync(AppId, "I would like a fullstack typescript programming with azure job", verbose: true);

            Assert.NotNull(result);
            Assert.True(result.TopScoringIntent.Intent == "GetJobInformation");
            Assert.True(result.TopScoringIntent.Score > 0.6);

            // Equivalent to 'IsEntityPresent(entityName)'
            Assert.True(result.Entities.FirstOrDefault(e => 
                (e.Entity == "fullstack typescript programming with azure" && e.StartIndex == 15)) != null,
                "Expected entity 'fullstack typescript programming with azure' at start index 15");
        }


        [Theory]
        [MemberData(memberName: nameof(LuisBatchTestData))]
        public async Task DoBatchTest(string utterance, string expectedIntent, List<Entity> entities)
        {
            // Create client with SuscriptionKey and AzureRegion
            var client = new LUISRuntimeClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
            {
                Endpoint = Endpoint
            };

            // Predict
            LuisResult result = await client.Prediction.ResolveAsync(AppId, utterance, verbose: true);

            Assert.NotNull(result);
            Assert.True(result.TopScoringIntent.Intent == expectedIntent);
            Assert.True(result.TopScoringIntent.Score > 0.6);

            foreach (var item in entities)
            {
                // Equivalent to 'IsEntityPresent(entityName)'
                Assert.True(result.Entities.FirstOrDefault(e => e.Type == item.entity) != null, $"Utterance: \"{utterance}\". Expected entity type {item.entity}, not present");
            }
        }

        public static IEnumerable<object[]> LuisBatchTestData
        {
            get
            {
                var jsondata = File.ReadAllText("BatchTestData.json");
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<LuisTestCaseData[]>(jsondata);
                var alldata = new List<object[]>();
                foreach (var item in data)
                {
                    alldata.Add(new object[] { item.text, item.intent, item.entities });
                }
                return alldata;
            }
        }
    }
}
