import * as msRest from 'ms-rest';
import LUISRuntimeClient from 'azure-cognitiveservices-luis-runtime';
import { LuisResult } from 'azure-cognitiveservices-luis-runtime/lib/models';

export interface LUISRuntimeClientSettings {
  token: string;
  endpoint: string;
  appId: string;
  subscriptionKey: string;
  timezoneOffset?: number;
  verbose?: boolean;
  staging?: boolean;
  log?: boolean;
}

require('dotenv').config();

const token = process.env.LUIS_TOKEN!;
const creds = new msRest.TokenCredentials(token);
const endpoint = process.env.LUIS_ENDPOINT!;
const client = new LUISRuntimeClient(creds, endpoint);
const appId = process.env.LUIS_APPID!;
const subscriptionKey = process.env.LUIS_SUBSCRIPTION_KEY!;
const timezoneOffset = 1.01;
const verbose = true;
const staging = true;
const log = true;

export async function predict(query: string): Promise<LuisResult> {
  try {
    return await client.prediction.resolve(appId, query, {
      timezoneOffset,
      verbose,
      staging,
      log,
      customHeaders: {
        'Ocp-Apim-Subscription-Key': subscriptionKey
      }
    });
  } catch (e) {
    throw new Error(e);
  }
}
