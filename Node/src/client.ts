import * as msRest from 'ms-rest';
import LUISRuntimeClient from 'azure-cognitiveservices-luis-runtime';
import { LuisResult } from 'azure-cognitiveservices-luis-runtime/lib/models';

export namespace LUISTestClient {
  export interface Settings {
    token: string;
    endpoint: string;
    appId: string;
    subscriptionKey: string;
    timezoneOffset?: number;
    verbose?: boolean;
    staging?: boolean;
    log?: boolean;
  }

  export interface Client {
    predict(query: string): Promise<LuisResult>;
  }
}

require('dotenv').config();

const envSettings: LUISTestClient.Settings = {
  token: process.env.LUIS_TOKEN!,
  endpoint: process.env.LUIS_ENDPOINT!,
  appId: process.env.LUIS_APPID!,
  subscriptionKey: process.env.LUIS_SUBSCRIPTION_KEY!,
  timezoneOffset: 1.01,
  verbose: true,
  staging: true,
  log: true
};

class LUISTestClient implements LUISTestClient.Client {
  constructor(
    public client: LUISRuntimeClient,
    public settings: LUISTestClient.Settings
  ) {}
  public async predict(query: string): Promise<LuisResult> {
    try {
      return await this.client.prediction.resolve(this.settings.appId, query, {
        timezoneOffset: this.settings.timezoneOffset,
        verbose: this.settings.verbose,
        staging: this.settings.staging,
        log: this.settings.log,
        customHeaders: {
          'Ocp-Apim-Subscription-Key': this.settings.subscriptionKey
        }
      });
    } catch (e) {
      throw new Error(e);
    }
  }
}

export function createClient(
  settings: LUISTestClient.Settings = envSettings
): LUISTestClient.Client {
  const creds = new msRest.TokenCredentials(settings.token);
  const client = new LUISRuntimeClient(creds, settings.endpoint);

  return new LUISTestClient(client, settings);
}
