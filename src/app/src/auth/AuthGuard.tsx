import { useIsAuthenticated } from "@azure/msal-react";
import { LoginButton } from "./LoginButton";
import type { ReactNode } from "react";

interface AuthGuardProps {
  children: ReactNode;
}

export const AuthGuard = ({ children }: AuthGuardProps) => {
  const isAuthenticated = useIsAuthenticated();

  if (!isAuthenticated) {
    return <LoginButton />;
  }

  return <>{children}</>;
};
