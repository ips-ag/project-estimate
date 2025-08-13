import { config } from "../config/config";
import { FileUploadResponse } from "../types";

export default class ApiService {
  private static async getAuthToken(): Promise<string | null> {
    try {
      // Import dynamically to avoid build errors when packages aren't installed yet
      const { PublicClientApplication } = await import("@azure/msal-browser");
      const { msalConfig } = await import("../auth/authConfig");
      
      const msalInstance = new PublicClientApplication(msalConfig);
      await msalInstance.initialize();
      
      const accounts = msalInstance.getAllAccounts();
      if (accounts.length === 0) {
        console.error('No accounts found in MSAL');
        return null;
      }

      const account = accounts[0];
      const tokenRequest = {
        scopes: ["openid", "profile", "email"],
        account: account
      };

      const response = await msalInstance.acquireTokenSilent(tokenRequest);
      console.log('Token acquired silently from MSAL');
      return response.accessToken;
    } catch (error) {
      console.error('Error acquiring token from MSAL:', error);
      return null;
    }
  }

  public static async uploadFile(file: File): Promise<FileUploadResponse> {
    try {
      const token = await this.getAuthToken();
      const headers: HeadersInit = {};
      
      if (token) {
        headers['Authorization'] = `Bearer ${token}`;
      }

      const formData = new FormData();
      formData.append("file", file, file.name);
      const response = await fetch(config.apiUrl + "/file", {
        method: "POST",
        headers: headers,
        body: formData,
      });
      return await response.json();
    } catch (error) {
      console.error("Error uploading file:", error);
      return {
        location: "",
        errorMessage: "Failed to upload file",
      };
    }
  }
}
