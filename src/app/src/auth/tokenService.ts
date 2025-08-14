import { PublicClientApplication, SilentRequest } from "@azure/msal-browser";
import { msalConfig } from "./authConfig";

export const getAccessToken = async (): Promise<string> => {
  try {
    const msalInstance = new PublicClientApplication(msalConfig);
    await msalInstance.initialize();

    const accounts = msalInstance.getAllAccounts();
    if (accounts.length === 0) {
      console.error("No accounts found in MSAL");
      return "";
    }

    const account = accounts[0];
    const tokenRequest: SilentRequest = {
      scopes: ["openid", "profile", "email", `${import.meta.env.VITE_AZURE_CLIENT_ID}/ProjectEstimate`],
      account: account,
    };

    const response = await msalInstance.acquireTokenSilent(tokenRequest);
    return response.accessToken;
  } catch (error) {
    console.error("Error acquiring access token", error);
    return "";
  }
};
