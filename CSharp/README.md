
# MSTest extensions to aid unit testing of LUIS apps

This library implements extensions to help to create unit tests for LUIS apps more easily. LUIS apps are tested by sending an utterance to the endpoint that is different from any utterances used to train the model, and then testing whether the expected Intent and/or entities are returned. This library supports testing in two different ways:

  1. **Single shot** Send a single utterance to the endpoint and test the response using Assert to verify that the response contains the expected intent and/or entities. You can test that the confidence rating for the intent exceeds a certain value. You can also test that the entities returned are only those expected and no false positives have been returned (i.e. unexpected entities).

  2. **Batch testing** Supply an input file for batch testing using the [format used by the online batch testing tool](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-concept-batch-test#batch-file-format). A single test case in here is defined as follows, where "text" is the utterance to send, "intent" is the expected intent, and "entities" defines each expected entity along with the start and end positions in the utterance:

  ```json
  [
    {
      "text": "I would like a fullstack typescript programming with azure job",
      "intent": "GetJobInformation",
      "entities": 
      [
          {
              "entity": "Job",
              "startPos": 15,
              "endPos": 46
          }
      ]
    }
  ]
  ```

  By declaring a single test method, every utterance in the input file will be tested to verify
    - The expected intent is returned
    - The expected entities are returned
    - No unexpected entities are returned
    - Optionally test that the intent confidence level returned for all test cases exceeds a specified value

## Development Notes

### Unit Testing without using any extensions

LUIS apps can be tested with single shot utterances without using anything from this library. This library adds some extensions to make this easier, and also adds an easy way to test using a batch test file.

Without using this library, the unit test developer can create a test class, and add a reference to the [Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime NuGet package](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime), and then write a test such as this:

```csharp
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
            // Create client with SubcriptionKey, and specifying the endpoint
            var client = new LUISRuntimeClient(new ApiKeyServiceClientCredentials(SubscriptionKey))
            {
                Endpoint = Endpoint
            };

            // Predict
            LuisResult result = await client.Prediction.ResolveAsync(AppId, "I would like a fullstack typescript programming with azure job", verbose: true);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.TopScoringIntent.Intent == "GetJobInformation");
            Assert.IsTrue(result.TopScoringIntent.Score > 0.6);

            // Equivalent to 'IsEntityPresent(entityName)'
            Assert.IsNotNull(result.Entities.Select(e => e.Entity == "Job").FirstOrDefault());
        }
```

Note that the subscription key, app ID and endpoint for the LUIS app under test should be specified in a .runsettings file. For example:
```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <!-- Parameters used by tests at runtime -->
  <TestRunParameters>
    <Parameter name="subscriptionKey" value="YOUR_SUBSCRIPTIONKEY" />
    <Parameter name="appId" value="YOUR_APPID" />
    <Parameter name="endpoint" value="YOUR_ENDPOINT e.g. https://westus.api.cognitive.microsoft.com" />
  </TestRunParameters>
</RunSettings>
```
You must then set this as the current test settings file when running tests.

### What does the library add?
The output for this project is a .NET Core library that will be published to NuGet as package *Microsoft.UnitTestFramework.Luis.Extensions*. This library implements methods that make the unit test developers' job a little easier for single shot testing, and a lot easier for batch testing. It implements the following:

  - **Assert.ReturnsIntent(LuisResult result, string intentName, double minimumConfidenceRating = 0.6)**
    Throws if the expected intent is not returned. Throws if the confidence rating is not greater than or equal to the specified value (optional, defaults to 0.6 if not supplied)

  - **Assert.ContainsEntity(LuisResult result, string entityName, int startPos = 0, int endPos = 0)**
    Throws if the specified entity is not returned. If the startPos and endPos are specified, additionally verifies that the entity position returned in the response matches.

  - **Assert.ReturnsOnlyEntities(LuisResult result, string[] entities)**
    Verifies that the response contains only those entities that are contained within the entties string array. Throws for both false negatives (a specified entity is missing) and for false positives (an entity is retuned that is not in the expected entities). 

*Developer note:* Look at [Microsoft Test Framework Extensions](https://github.com/Microsoft/mstest-extensions) maybe for help with creating custom Asserts.

### Batch Testing

A developer can also easily create a test that uses an utterance [batch test file](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-concept-batch-test#batch-file-format) as input, by using the **RunBatchTest** method as follows:

```csharp
        [TestMethod()]
        [DeploymentItem("mybatchtestfile.json")]
        public void BatchFileTest()
        {
            string file = "mybatchtestfile.json";
            Microsoft.UnitTestFramework.Luis.Extensions.RunBatchTest(file, subscription, appId, endpoint);
        }
```

What this does:
  - Assert.IsTrue(File.Exists(file), "deployment failed: " + file + " did not get deployed");
  - Reads the input file and for each test case:
    - Assert.ReturnsIntent(...)
    - Assert.ContainsEntity(...) - for each entity specified in the test case, tests also for position
    - Assert.ContainsOnlyEntities(...)
