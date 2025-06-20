import "./DebugToggle.css";

type DebugToggleProps = {
  showDebugMessages: boolean;
  onDebugToggle: (enabled: boolean) => void;
};

export default function DebugToggle({ showDebugMessages, onDebugToggle }: DebugToggleProps) {
  return (
    <div className="debug-toggle">
      <label htmlFor="debugToggle" className="debug-toggle-label">
        <input
          id="debugToggle"
          type="checkbox"
          checked={showDebugMessages}
          onChange={(e) => onDebugToggle(e.target.checked)}
          className="debug-toggle-checkbox"
        />
        <span className="debug-toggle-text">Show debug messages</span>
      </label>
    </div>
  );
}
