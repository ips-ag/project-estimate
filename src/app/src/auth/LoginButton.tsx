import { useMsal } from "@azure/msal-react";
import { InteractionStatus } from "@azure/msal-browser";
import { loginRequest } from "./authConfig";

export const LoginButton = () => {
  const { instance, inProgress } = useMsal();
  const isInteractionInProgress = inProgress !== InteractionStatus.None;

  const handleLogin = () => {
    if (isInteractionInProgress) {
      return;
    }

    instance.loginPopup(loginRequest)
      .catch((e) => {
        console.error('Login error:', e);
      });
  };

  return (
    <div style={{ 
      display: 'flex', 
      justifyContent: 'center', 
      alignItems: 'center', 
      height: '100vh',
      flexDirection: 'column',
      gap: '20px'
    }}>
      <h1>Project Estimate</h1>
      <p>Please sign in to continue</p>
      <button
        onClick={handleLogin}
        disabled={isInteractionInProgress}
        style={{
          padding: '12px 24px',
          fontSize: '16px',
          backgroundColor: '#0078d4',
          color: 'white',
          border: 'none',
          borderRadius: '4px',
          cursor: isInteractionInProgress ? 'default' : 'pointer',
          opacity: isInteractionInProgress ? 0.7 : 1,
          transition: 'background-color 0.2s'
        }}
        onMouseEnter={(e) => { if (!isInteractionInProgress) e.currentTarget.style.backgroundColor = '#106ebe'; }}
        onMouseLeave={(e) => { if (!isInteractionInProgress) e.currentTarget.style.backgroundColor = '#0078d4'; }}
      >
        {isInteractionInProgress ? 'Signing in...' : 'Sign in with Microsoft Entra ID'}
      </button>
    </div>
  );
};
