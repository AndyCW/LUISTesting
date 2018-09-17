import { createClient } from './client';
import { readFileSync } from 'fs';
import * as path from 'path';
import {
  LuisResult,
  EntityModel
} from 'azure-cognitiveservices-luis-runtime/lib/models';
namespace LUISTestCase {
  export interface Entity {
    entity: string;
    startPos: number;
    endPos: number;
  }

  export interface Data {
    text: string;
    intent: string;
    entities: EntityModel[];
  }
}

type LUISBatchTestCases = LUISTestCase.Data[];

const TestLuisClient = createClient(); // uses env settings by default

export async function testExpectedIntent(
  luisTestCaseData: LUISTestCase.Data
): Promise<true> {
  const result = await TestLuisClient.predict(luisTestCaseData.text);

  const { topScoringIntent } = result;

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

  testEntitiesForFalseNegatives(
    result,
    luisTestCaseData.text,
    luisTestCaseData.entities
  );

  return true;
}

export function testEntitiesForFalseNegatives(
  result: LuisResult,
  utterance: string,
  expectedEntities: EntityModel[]
): boolean {
  // const errors: string[] = [];
  expectedEntities.forEach(expectedEntity => {
    const { startIndex, endIndex } = expectedEntity;
    const entityValue = utterance.slice(startIndex, endIndex - startIndex);

    if (entityValue !== expectedEntity.entity) {
      throw new Error(`${entityValue} != ${expectedEntity.entity}`);
    }

    testIsEntityPresent(result, utterance, entityValue);
  });

  return true;
}

export function testIsEntityPresent(
  result: LuisResult,
  utterance: string,
  expectedEntity: string,
  expectedStartIndex: number = -1
): boolean {
  if (!result.entities) {
    throw new Error('no entities present');
  }

  const customEntities = (result.entities || [])
    .filter(item => {
      if (item.type.indexOf('builtin') !== 0) {
        return true;
      }
    })
    .map(item => item.entity);

  let message = `Utterance: "${utterance}" Expected entity "${expectedEntity}"`;

  if (expectedStartIndex > -1) {
    message += ` at start index ${expectedStartIndex}`;
  }
  message += `, actual entities: [${customEntities.join("' ")}]`;

  if (expectedStartIndex > -1) {
    if (
      result.entities.find(e => {
        return (
          e.entity === expectedEntity && e.startIndex === expectedStartIndex
        );
      }) === undefined
    ) {
      throw new Error(message);
    }
  } else {
    if (result.entities.find(e => e.entity === expectedEntity) === undefined) {
      throw new Error(message);
    }
  }

  return true;
}

export function testExpectedMinConfidenceScore(result: LuisResult, utterance: string, minConfidence: number): boolean {
  if (result.topScoringIntent) {
    if (result.topScoringIntent.score! < minConfidence) {
      throw new Error(`Utterance: "${utterance}" Top scoring Intent confidence < ${minConfidence}, actual: ${result.topScoringIntent.score}`)
    }
  } else {
    throw new Error(`Utterance: "${utterance}" Top scoring Intent missing. Expected: intent with score > ${minConfidence}`);
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
        return await testExpectedIntent(testCase);
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
