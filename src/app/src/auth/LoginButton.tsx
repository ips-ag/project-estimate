import { useMsal } from "@azure/msal-react";
import { loginRequest } from "./authConfig";

export const LoginButton = () => {
  const { instance } = useMsal();

  const handleLogin = () => {
    console.log('Starting login process');
    
    instance.loginPopup(loginRequest)
      .then(() => {
        console.log('Login successful');
      })
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
        style={{
          padding: '12px 24px',
          fontSize: '16px',
          backgroundColor: '#0078d4',
          color: 'white',
          border: 'none',
          borderRadius: '4px',
          cursor: 'pointer',
          transition: 'background-color 0.2s'
        }}
        onMouseEnter={(e) => e.currentTarget.style.backgroundColor = '#106ebe'}
        onMouseLeave={(e) => e.currentTarget.style.backgroundColor = '#0078d4'}
      >
        Sign in with Microsoft
      </button>
    </div>
  );
};
