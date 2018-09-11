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

type BatchLUISTestCases = LUISTestCase.Data[];

const TestLuisClient = createClient(); // uses env settings by default

export async function intentMatches(
  luisTestCaseData: LUISTestCase.Data
): Promise<true> {
  const { topScoringIntent } = await TestLuisClient.predict(
    luisTestCaseData.text
  );

  if (!topScoringIntent) {
    throw new Error('No top scoring intent present.');
  }

  if (topScoringIntent.intent !== luisTestCaseData.intent) {
    const errorMessage = [
      `Test case for "${luisTestCaseData.text}" failed.`,
      'Intents do not match.',
      `Expected: ${luisTestCaseData.intent}`,
      `Actual: ${topScoringIntent.intent}`
    ].join('\n');
    throw new Error(errorMessage);
  }

  return true;
}

export async function intentsMatch(
  batch: string | BatchLUISTestCases
): Promise<true> {
  let batchData: BatchLUISTestCases;
  if (typeof batch === 'string') {
    const rawData = readFileSync(path.join(__dirname, batch), 'utf-8');
    batchData = JSON.parse(rawData) as BatchLUISTestCases;
  } else {
    batchData = batch;
  }

  await Promise.all(
    batchData.map(async testCase => {
      return await intentMatches(testCase);
    })
  );

  return true;
}

intentsMatch('batchTestData.json').then(value => {
  console.log(value);
});
