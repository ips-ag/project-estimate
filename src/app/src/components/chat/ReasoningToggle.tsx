import "./ReasoningToggle.css";

type ReasoningToggleProps = {
  showReasoning: boolean;
  onReasoningToggle: (enabled: boolean) => void;
};

export default function ReasoningToggle({ showReasoning, onReasoningToggle }: ReasoningToggleProps) {
  return (
    <div className="reasoning-toggle">
      <label htmlFor="reasoningToggle" className="reasoning-toggle-label">
        <input
          id="reasoningToggle"
          type="checkbox"
          checked={showReasoning}
          onChange={(e) => onReasoningToggle(e.target.checked)}
          className="reasoning-toggle-checkbox"
        />
        <span className="reasoning-toggle-text">Show reasoning</span>
      </label>
    </div>
  );
}
