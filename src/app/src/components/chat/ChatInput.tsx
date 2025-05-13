import { useState } from "react";
import type { KeyboardEvent } from "react";
import FileSelect from "./FileSelect";
import sendIcon from "../../assets/send.svg";
import spinnerIcon from "../../assets/spinner.svg";
import "./ChatInput.css";

type ChatInputProps = {
  isLoading: boolean;
  isUploading: boolean;
  hasInputFile: boolean;
  onFileSelected: (file: File) => void;
  onSend: (message: string) => void;
};

export default function ChatInput({ isLoading, isUploading, hasInputFile, onFileSelected, onSend }: ChatInputProps) {
  const [userInput, setUserInput] = useState("");

  const handleUserInputChange = (input: string) => {
    setUserInput(input);
  };

  const handleSubmit = () => {
    if (!userInput.trim() && !hasInputFile) return;
    onSend(userInput);
    setUserInput("");
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSubmit();
    }
  };

  return (
    <div className="chat-form">
      <FileSelect
        onFileSelected={onFileSelected}
        isUploading={isUploading}
        isDisabled={isUploading || isLoading}
        hasUploadedFile={hasInputFile}
      />

      <textarea
        id="userInput"
        disabled={isLoading}
        className="chat-textarea"
        value={userInput}
        onChange={(e) => handleUserInputChange(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="Enter requirements or select a file..."
      />

      <button
        id="submitButton"
        className="submit-button"
        type="button"
        onClick={handleSubmit}
        disabled={isLoading || (!userInput.trim() && !hasInputFile)}
      >
        {isLoading ? (
          <img src={spinnerIcon} alt="Loading..." className="button-icon spinner" />
        ) : (
          <img src={sendIcon} alt="Send" className="button-icon" />
        )}
      </button>
    </div>
  );
}
