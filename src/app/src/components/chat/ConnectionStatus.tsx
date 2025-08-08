import "./ConnectionStatus.css";
import { ConnectionState } from "../../types";

interface ConnectionStatusProps {
  status: ConnectionState;
}

export default function ConnectionStatus({ status }: ConnectionStatusProps) {
  const getStatusClass = () => {
    switch (status) {
      case ConnectionState.Connected:
        return 'connected';
      case ConnectionState.Connecting:
        return 'connecting';
      case ConnectionState.Disconnected:
        return 'disconnected';
      default:
        return 'disconnected';
    }
  };

  const getStatusTitle = () => {
    switch (status) {
      case ConnectionState.Connected:
        return 'Chat Connected';
      case ConnectionState.Connecting:
        return 'Chat Connecting...';
      case ConnectionState.Disconnected:
        return 'Chat Disconnected';
      default:
        return 'Chat Disconnected';
    }
  };

  return (
    <div className="connection-status">
      <div
        className={`connection-indicator ${getStatusClass()}`}
        title={getStatusTitle()}
      >
        <div className="connection-circle"></div>
      </div>
    </div>
  );
}
