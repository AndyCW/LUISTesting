using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using System.Threading.Tasks;
using System.Linq;

namespace LUISUnitTestsTester
{
    /// <summary>
    /// Example test class to test a LUIS app
    /// 
    /// IMPORTANT: To test a LUIS app, you must supply the Subscription Key, App ID and the 
    /// endpoint where it is published. Enter these values into the luistesting.runsettings file
    /// in this project, then make sure that you select that file as the Test Settings file by using the
    /// Test - Test Settings - Select Test Settings File menu option in Visual Studio.
    /// Using a .runsettings file allows values to be overridden in an automated CI build in VSTS.
    /// </summary>
    [TestClass]
    public class UnitTest1
    {
        private static string SubscriptionKey;
        private static string AppId;
        private static string Endpoint;

        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        /// <summary>
        /// Run before all tests
        /// Make sure you have entered the ettings for your LUIS app in the luistesting.runsettings file, and
        /// selected that file as the current Test Settings file from the Test - Test Settings menu
        /// </summary>
        /// <param name="context"></param>
        [ClassInitialize]
        public static void TestInitialize(TestContext context)
        {
            SubscriptionKey = context.Properties["subscriptionKey"].ToString();
            AppId = context.Properties["appId"].ToString();
            Endpoint = context.Properties["endpoint"].ToString();
        }

        [TestMethod]
        public async Task TestMethod1()
        {
            // Create client with SuscriptionKey and AzureRegion
            var client = new LUISRuntimeClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
            {
                Endpoint = Endpoint
            };

            // Predict
            LuisResult result = await client.Prediction.ResolveAsync(AppId, "I'd like a Big Mac, fries and a coke, please", verbose: true);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.TopScoringIntent.Intent == "order-items");
            Assert.IsTrue(result.TopScoringIntent.Score > 0.6);

            // Equivalent to 'IsEntityPresent(entityName)'
            Assert.IsNotNull(result.Entities.Select(e => e.Entity == "item").FirstOrDefault());
        }
    }
}
