import { useIsAuthenticated } from "@azure/msal-react";
import { useEffect } from "react";
import { LoginButton } from "./LoginButton";
import type { ReactNode } from "react";

interface AuthGuardProps {
  children: ReactNode;
  onAuthenticated?: () => void;
}

export const AuthGuard = ({ children, onAuthenticated }: AuthGuardProps) => {
  const isAuthenticated = useIsAuthenticated();

  useEffect(() => {
    if (isAuthenticated && onAuthenticated) {
      onAuthenticated();
    }
  }, [isAuthenticated, onAuthenticated]);

  if (!isAuthenticated) {
    return <LoginButton />;
  }

  return <>{children}</>;
};
