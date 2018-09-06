
# Libraries to assist with unit testing of LUIS apps

This library implements extensions to help to create unit tests for LUIS apps more easily. LUIS apps are tested by sending an utterance to the endpoint that is different from any utterances used to train the model, and then testing whether the expected Intent and/or entities are returned. This library supports testing in two different ways:

  1. **Single shot** Send a single utterance to the endpoint and test the response using Assert to verify that the response contains the expected intent and/or entities. You can test that the confidence rating for the intent exceeds a certain value. You can also test that the entities returned are only those expected and no false positives have been returned (i.e. unexpected entities).

  2. **Batch testing** Supply an input file for batch testing using the [format used by the online batch testing tool](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/luis-concept-batch-test#batch-file-format).
  
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
