using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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

        /// <summary>
        /// Example of a single shot test of a LUIS app
        /// Defines the utterance to send and tests in the result that the expected Intent was returned, 
        /// that the confidence score exceeds a certain value and that a specific entity was identified 
        /// by the LUIS app, starting at a particular position in the utterance.
        /// </summary>
        /// <returns>Asserts if test cases fail</returns>
        [Fact]
        public async Task SingleUtteranceTest()
        {
            // Create client with SuscriptionKey and AzureRegion
            var client = new LUISRuntimeClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
            {
                Endpoint = Endpoint
            };

            // Predict
            string testUtterance = "Tell me about fullstack typescript programming with azure jobs";
            LuisResult result = await client.Prediction.ResolveAsync(AppId, testUtterance, verbose: true);

            Assert.NotNull(result);
            TestExpectedIntent(result, testUtterance, "GetJobInformation");
            TestExpectedMinConfidenceScore(result, testUtterance, 0.6);
            TestIsEntityPresent(result, testUtterance, "fullstack typescript programming", 14);
        }

        /// <summary>
        /// Simple example of a unit test method that does batch testing of a LUIS app. 
        /// The tests are defined in a json input file - BatchTestData.json in this example
        /// </summary>
        /// <param name="utterance">the utterance to send to the LUIS app</param>
        /// <param name="expectedIntent">the expected top intent returned in the LuisResult</param>
        /// <param name="expectedEntities">the expected simple entities returned in the LuisResult</param>
        /// <returns>Asserts in each test case if the expected intent and expected entities are not returned</returns>
        [Theory]
        [MemberData(memberName: nameof(LuisBatchTestData))]
        public async Task DoBatchTest(string utterance, string expectedIntent, List<Entity> expectedEntities)
        {
            // Create client with SuscriptionKey and AzureRegion
            var client = new LUISRuntimeClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
            {
                Endpoint = Endpoint
            };

            // Predict
            LuisResult result = await client.Prediction.ResolveAsync(AppId, utterance, verbose: true);

            // First validate that the expected intent was returned
            Assert.NotNull(result);
            TestExpectedIntent(result, utterance, expectedIntent);
            // Validate that all the simple entities were identified that we expected
            TestEntitiesForFalseNegatives(result, utterance, expectedEntities);
            // Validate that no simple entities were identified that we do not expect
            TestEntitiesForFalsePositives(result, utterance, expectedEntities);
        }

        /// <summary>
        /// Property that exposes the test cases read from the file as IEnumerable.
        /// For each of the test cases in the file, the test framework calls unit test methods that have
        /// a MemberDataAttribute referencing this property, passing the top level properties of each test case object
        /// (utterance, expectedIntent and expecetedEntities in this case) as parameters to the unit test method
        /// </summary>
        public static IEnumerable<object[]> LuisBatchTestData
        {
            get
            {
                var jsondata = File.ReadAllText("BatchTestData.json");
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<LuisTestCaseData[]>(jsondata);
                var alldata = new List<object[]>();
                foreach (var item in data)
                {
                    alldata.Add(new object[] { item.Utterance, item.Intent, item.Entities });
                }
                return alldata;
            }
        }

        /// <summary>
        /// Tests that an entity value is present in the entities returned in the LUIS result,
        /// optionally tests the start position of the entity.
        /// Asserts if the tests fail.
        /// </summary>
        /// <param name="result">the LuisResult returned from a call to LUIS</param>
        /// <param name="utterance">utterance passed to LUIS</param>
        /// <param name="expectedEntity">value of the entity that should be in the LuisResult entities</param>
        /// <param name="expectedStartIndex">[Optional] if zero or higher, tests that the entity is detected at the expecetdStartIndex</param>
        private static void TestIsEntityPresent(LuisResult result, string utterance, string expectedEntity, int expectedStartIndex = -1)
        {
            List<string> customEntities = new List<string>();
            foreach (var item in result.Entities)
            {
                if (!item.Type.StartsWith("builtin")) customEntities.Add(item.Entity);
            }

            string message = $"Utterance: \"{utterance}\" Expected entity '{expectedEntity}'";
            if (expectedStartIndex > -1) message += $" at start index {expectedStartIndex}";
            message += $", actual entities: [{String.Join("' ", customEntities)}]";

            if (expectedStartIndex > -1)
            {
                Assert.True(result.Entities.FirstOrDefault(e =>
                    (e.Entity == expectedEntity && e.StartIndex == expectedStartIndex)) != null,
                    message);
            }
            else
            {
                Assert.True(result.Entities.FirstOrDefault(e =>
                    (e.Entity == expectedEntity)) != null,
                    message);
            }
        }

        /// <summary>
        /// Tests that the confidence score for the top intent exceeds a specified value.
        /// Asserts if the test fails.
        /// </summary>
        /// <param name="result">LuisResult object returned from a call to LUIS</param>
        /// <param name="utterance">the utterance that was passed to LUIS</param>
        /// <param name="minConfidence">the minimum confidence score that is valid</param>
        private static void TestExpectedMinConfidenceScore(LuisResult result, string utterance, double minConfidence)
        {
            Assert.True(result.TopScoringIntent.Score > minConfidence, $"Utterance: \"{utterance}\" Top scoring Intent confidence < {minConfidence}, actual {result.TopScoringIntent.Score}");
        }

        /// <summary>
        /// Tests that the top intent returned from a LUIS call matches the test value.
        /// Asserts if the test fails.
        /// </summary>
        /// <param name="result">LuisResult object returned from a call to LUIS</param>
        /// <param name="utterance">the utterance that was passed to LUIS</param>
        /// <param name="expectedIntent">The expected top intent name</param>
        private static void TestExpectedIntent(LuisResult result, string utterance, string expectedIntent)
        {
            Assert.True(result.TopScoringIntent.Intent == expectedIntent, $"Utterance: \"{utterance}\" Expected intent {expectedIntent}, actual {result.TopScoringIntent.Intent}");
        }

        /// <summary>
        /// Given a List of Entity objects representing the simple entities expected to be returned in the LuisResponse,
        /// tests that all the specified entities are returned in the LuisResponse.
        /// Asserts if any of the specified entities are not returned in the LuisResponse.
        /// </summary>
        /// <param name="result">LuisResult object returned from a call to LUIS</param>
        /// <param name="utterance">the utterance that was passed to LUIS</param>
        /// <param name="expectedEntities">List of Entity objects that are expected to be in the response</param>
        private static void TestEntitiesForFalseNegatives(LuisResult result, string utterance, List<Entity> expectedEntities)
        {
            // Test for false negatives - expected entity not found in the LuisResult entities
            foreach (var expectedEntity in expectedEntities)
            {
                // The batch test data file includes the entity name, but not the value - get that using the start and end index
                string entityValue = utterance.Substring(expectedEntity.StartPosition, expectedEntity.EndPosition - expectedEntity.StartPosition + 1);
                TestIsEntityPresent(result, utterance, entityValue);
            }
        }

        /// <summary>
        /// Given a List of Entity objects representing the simple entities expected to be returned in the LuisResponse,
        /// tests that only those simple entities are returned in the LuisResponse, and that no additional simple entities 
        /// are returned from LUIS.
        /// Asserts if any additional, unexpected simple entities are returned in the LuisResponse.
        /// </summary>
        /// <param name="result">LuisResult object returned from a call to LUIS</param>
        /// <param name="utterance">the utterance that was passed to LUIS</param>
        /// <param name="expectedEntities"></param>
        private static void TestEntitiesForFalsePositives(LuisResult result, string utterance, List<Entity> expectedEntities)
        {
            // Test for false positives - entity in the LuisResult not found in the expected entities
            List<string> expectedEntityValues = new List<string>();
            foreach (var expectedEntity in expectedEntities)
            {
                // The batch test data file includes the entity name, but not the value - get that using the start and end index
                string entityValue = utterance.Substring(expectedEntity.StartPosition, expectedEntity.EndPosition - expectedEntity.StartPosition + 1);
                // Build up the list of entity values so we can do a false positives test
                expectedEntityValues.Add(entityValue);
            }
            foreach (var actualEntity in result.Entities)
            {
                // Test for entities, excluding Prebuilts
                if (!actualEntity.Type.StartsWith("builtin"))
                {
                    bool isEntityPresent = expectedEntityValues.FirstOrDefault(e => e == actualEntity.Entity) != null;
                    Assert.True(isEntityPresent, $"Utterance \"{utterance}\". False Positive - entity value \"{actualEntity.Entity}\" returned from Luis, not in expected entities set of [{String.Join("' ", expectedEntityValues)}]");
                }
            }
        }

    }
}
