
# Sample: unit testing of LUIS apps

LUIS apps can be maintained in source control systems by placing the json export file in your SCC, in just the same way as any other software artefact. Branching, deployment to different dev environments such as QA or Staging, and CI builds can also be supported by creating a new instance of the LUIS app b y importing the json file from the source. In this way, development and testing may proceed independently of whatever is happening in other 'branches'.

Any development of a LUIS app using 'proper' ALM practices requires the development of unit testing code. For a LUIS app, this means  testing in two different ways:

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

## How to Unit Testing LUIS using xUnit

The unit test developer can create an xUnit test project, and add a reference to the [Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime NuGet package](https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime), and then write tests as demonstrated by the sample in this folder.


