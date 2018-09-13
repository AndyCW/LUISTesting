
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

# Tips on creating a Batch Test file
A batch test file contains one or more test cases defining the utterance to send to LUIS, the Intent name you expect to get back, and the entities you expect to get back. The utterances should be different from those used to train the LUIS app to have any value as valid test cases. For each entity, the test file specifies the entity type and the start and end index where the enity is found in the utterance string. Notice that the entity value is not defined, although that can easily be determined by doing a simple string extraction with the start and end index in the utterance string.
Here is an example of a single test case:

```json
[
  {
    "text": "Are there any janitorial jobs currently open?",
    "intent": "GetJobInformation",
    "entities": [
      {
        "entity": "Job",
        "startPos": 14,
        "endPos": 23
      }
    ]
  },
...
]
```
In this case, the utterance is sent to the LUIS app, and the test expects the response to show that the top intent is the **GetJobInformation** intent, and that a single simple entity will be returned of type **Job** which is at start index 14 and end index 23, which equates to the entity value of *janitorial*.

Note that a batch test case cannot specify builtin entity types, such as builtin.DateTimeV2 or builtin.keyPhrase - only simple, heirarchical parents and composite entities may be specified. [Read more about batch test files in the LUIS documentation here.](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-concept-batch-test#entities-allowed-in-batch-tests).

In a software development project using correct devOps practices, both the json export file defining your LUIS app and your test code and test files such as a batch test file should all be kept in an SCC (Source Code Control) system and are valuable assets for defining the progress through the development of your solution, and the test code becomes a crucial resource for ensuring that the LUIS app continues to perform correctly as it goes through different iterations.

## Maintaining a Batch Test file
You could of course maintain a batch test file using a text editor such as VS Code. However, the need to specify entities with startPos and endPos correct could make this a tedius and error prone operation.

A good way of maintaining a batch test file is to [use the authoring tools offered by the LUIS.ai portal](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-how-to-add-example-utterances). 
Use these steps to setup a LUIS app specifically for maintaining a  batch test set:
  1. Create a new LUIS app you will use just for the batch test cases. If the LUIS test target app has already undergone some development, then you could export the test target app, use a text editor to edit the export file and delete all the utterances defined to train the app, then import the resultant file to create the new app.
  1. Define Entities in the test app to match those in the test target, but only simple, heirarchical parents and composite entities. Do not bother to define any builtin entities as these are not allowed in a batch test file. 
  **NOTE** As development of the project progresses and new entities are defined in the test target LUIS app, you will have to kepp the test file app updated with the same set of simple, heirarchical parents and composiste entities defined in the test target. 
  1. Define the same Intents in the test app as those that exist for the test target.
  1. Create utterances for the appropriate Intent for each test you want to run in the batch test. [use the authoring tools](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-how-to-add-example-utterances) to tag the entities that you expect to get back.
  1. You do not need to train or publish the app.
  1. [Export](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-how-to-start-new-app#export-app) the test app.
  1. Use a text editor to edit the export file. Select all the json that defines the utterances, excluding the **&quot;utterances&quot;:** field label and save it to a new file. This file is your batch test file.

<pre><code>
<i>{
  "luis_schema_version": "3.0.0",
  "versionId": "0.2",
  "name": "HumaResourcesTest",
  "desc": "",
  "culture": "en-us",
  "intents": [
    {
      "name": "ApplyForJob"
    },
    {
      "name": "EmployeeFeedback"
    },
    {
      "name": "FindForm"
    },
    {
      "name": "GetJobInformation"
    },
    {
      "name": "MoveEmployee"
    },
    {
      "name": "None"
    }
  ],
  "entities": [
    {
      "name": "Job",
      "roles": []
    },
    {
      "name": "Locations",
      "children": [
        "Destination",
        "Origin"
      ],
      "roles": []
    }
  ],
  "composites": [
    {
      "name": "requestemployeemove",
      "children": [
        "datetimeV2",
        "Locations::Destination",
        "Employee",
        "Locations::Origin"
      ],
      "roles": []
    }
  ],
  ...
  "regex_features": [],
  "patterns": [],
  "utterances":</i> <b>[
    {
      "text": "are there any janitorial jobs currently open?",
      "intent": "GetJobInformation",
      "entities": [
        {
          "entity": "Job",
          "startPos": 14,
          "endPos": 23
        }
      ]
    },
    {
      "text": "i would like a fullstack typescript programming with azure job",
      "intent": "GetJobInformation",
      "entities": [
        {
          "entity": "Job",
          "startPos": 15,
          "endPos": 46
        }
      ]
    },
    {
      "text": "is there a database position open in los colinas?",
      "intent": "GetJobInformation",
      "entities": [
        {
          "entity": "Job",
          "startPos": 11,
          "endPos": 18
        }
      ]
    },
    {
      "text": "please find database jobs open today in seattle",
      "intent": "GetJobInformation",
      "entities": [
        {
          "entity": "Job",
          "startPos": 12,
          "endPos": 19
        }
      ]
    }
  ]</b>
<i>}</i>
</code></pre>