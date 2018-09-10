import { createClient } from './client';

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
    throw new Error(
      `Intents do not match.\nExpected: ${luisTestCaseData.intent}\nActual: ${
        topScoringIntent.intent
      }`
    );
  }

  return true;
}

intentMatches({
  text: 'i need help',
  intent: 'Help',
  entities: []
}).then(value => {
  console.log(value);
});
