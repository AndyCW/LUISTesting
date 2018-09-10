import { predict } from './client';

namespace LuisTestCase {
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

export async function intentMatches(
  luisTestCaseData: LuisTestCase.Data
): Promise<true> {
  const { topScoringIntent } = await predict(luisTestCaseData.text);

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
