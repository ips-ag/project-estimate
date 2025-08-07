import "./ConnectionStatus.css";

interface ConnectionStatusProps {
  isConnected: boolean;
}

export default function ConnectionStatus({ isConnected }: ConnectionStatusProps) {
  return (
    <div className="connection-status">
      <div
        className={`connection-indicator ${isConnected ? "connected" : "disconnected"}`}
        title={isConnected ? "Chat Connected" : "Chat Not Connected"}
      >
        <div className="connection-circle"></div>
      </div>
    </div>
  );
}
