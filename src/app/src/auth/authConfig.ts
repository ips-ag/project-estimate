import { LogLevel, type Configuration, type PopupRequest } from "@azure/msal-browser";

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AZURE_CLIENT_ID,
    authority: import.meta.env.VITE_AZURE_AUTHORITY,
    redirectUri: window.location.origin,
    postLogoutRedirectUri: window.location.origin,
    navigateToLoginRequestUrl: false,
  },
  cache: {
    cacheLocation: "sessionStorage",
    storeAuthStateInCookie: false,
  },
  system: {
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) {
          return;
        }
        switch (level) {
          case LogLevel.Error:
            console.error(message);
            return;
          case LogLevel.Warning:
            console.warn(message);
            return;
          case LogLevel.Info:
            console.info(message);
            return;
          case LogLevel.Verbose:
            console.debug(message);
            return;
        }
      },
    },
  },
};

export const loginRequest: PopupRequest = {
  scopes: ["openid", "profile", "email", `${import.meta.env.VITE_AZURE_CLIENT_ID}/ProjectEstimate`],
};
