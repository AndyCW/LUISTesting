import { createClient } from './client';
import { readFileSync } from 'fs';
import * as path from 'path';
namespace LUISTestCase {
  export interface Entity {
    entity: string;
    startPos: number;
    endPos: number;
  }

  export interface Data {
    text: string;
    intent: string;
    entities: Entity[];
  }
}

type LUISBatchTestCases = LUISTestCase.Data[];

const TestLuisClient = createClient(); // uses env settings by default

export async function intentMatches(
  luisTestCaseData: LUISTestCase.Data
): Promise<true> {
  const { topScoringIntent } = await TestLuisClient.predict(
    luisTestCaseData.text
  );

  let errorMessage: string | undefined;

  if (!topScoringIntent) {
    errorMessage = 'No top scoring intent present.';
  } else if (topScoringIntent.intent !== luisTestCaseData.intent) {
    errorMessage = [
      `Intents do not match.`,
      `\tExpected: ${luisTestCaseData.intent}`,
      `\tActual: ${topScoringIntent.intent}`
    ].join('\n');
  } else {
    errorMessage = undefined;
  }

  if (errorMessage) {
    throw new Error(
      `Test case for "${luisTestCaseData.text}" failed: ${errorMessage}`
    );
  }

  return true;
}

interface LUISTestFailure {
  index: number;
  message: string;
}

export async function intentsMatch(
  batch: string | LUISBatchTestCases
): Promise<true> {
  let batchData: LUISBatchTestCases;
  if (typeof batch === 'string') {
    const rawData = readFileSync(path.join(__dirname, batch), 'utf-8');
    batchData = JSON.parse(rawData) as LUISBatchTestCases;
  } else {
    batchData = batch;
  }

  const testResults: LUISTestFailure[] = [];

  await Promise.all(
    batchData.map(async (testCase, index) => {
      try {
        return await intentMatches(testCase);
      } catch (e) {
        testResults.push({
          index,
          message: e.message
        });
      }
    })
  );

  if (testResults.length) {
    const aggregateErrorMessage = [
      `The following ${testResults.length} tests failed:`,
      ...testResults
        .sort((a, b) => a.index - b.index)
        .map(testResult => testResult.message)
    ].join('\n\n');

    throw new Error(aggregateErrorMessage);
  }

  return true;
}

intentsMatch('batchTestData.json').then(value => {
  console.log(value);
});
