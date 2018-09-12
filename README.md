
# Samples: unit testing of LUIS apps

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
  This repo contains samples demonstrating how to do unit testing using xUnit in MSTest, or using node.js.

# Running the samples

Create the Human Resources app from the [Improve app with batch test](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-tutorial-batch-testing) tutorial. You can find a copy of the json for that app here in this repo in file **HumanResources.json** or download from the Microsoft/LUIS-Samples Github repository [here](https://github.com/Microsoft/LUIS-Samples/blob/master/documentation-samples/tutorials/custom-domain-batchtest-HumanResources.json). [Import](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-how-to-start-new-app#import-new-app) the JSON into a new app in the [LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-reference-regions#luis-website) website. 

After you have imported the app, Train it and then Publish it. On the Publish page in the LUIS website, copy the endpoint URI from the bottom of the page in the **Resources and Keys** section. You need to extract the Subscription Key, the App ID and the endpoint URI root from the URI as follows:

For the example URI of https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/efb7acc7-7611-4098-babf-07561e45457c?subscription-key=edf60201edf4478e9ce785cd65e06a0f&verbose=true&timezoneOffset=0&q= 
  - Subscription Key is *edf60201edf4478e9ce785cd65e06a0f*
  - App ID is *efb7acc7-7611-4098-babf-07561e45457c*
  - Endpoint is *https://westus.api.cognitive.microsoft.com*

Note the values for your app and insert them into your **.env** file (for Node.js), or your **Properties/launchSettings.json** file for xUnit
  
# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
