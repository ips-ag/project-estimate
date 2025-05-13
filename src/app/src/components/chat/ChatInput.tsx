import { useState } from "react";
import type { KeyboardEvent } from "react";
import FileUploadButton from "./FileUploadButton";
import sendIcon from "../../assets/send.svg";
import spinnerIcon from "../../assets/spinner.svg";
import "./ChatInput.css";

type ChatInputProps = {
  isLoading: boolean;
  isUploading: boolean;
  fileInputLocation: string | undefined;
  onFileSelected: (file: File) => void;
  onSend: (message: string) => void;
};

export default function ChatInput({
  isLoading,
  isUploading,
  fileInputLocation,
  onFileSelected,
  onSend
}: ChatInputProps) {
  // Manage input state internally
  const [userInput, setUserInput] = useState("");

  const handleUserInputChange = (input: string) => {
    setUserInput(input);
  };

  const handleSubmit = () => {
    if (!userInput.trim() && !fileInputLocation) return;
    
    // Send the message to parent component
    onSend(userInput);
    
    // Reset state after sending
    setUserInput("");
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    // Submit on Enter without Shift key
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault(); // prevent adding a new line
      handleSubmit();
    }
  };

  return (
    <div className="chat-form">
      <FileUploadButton
        onFileSelected={onFileSelected}
        isUploading={isUploading}
        isDisabled={isUploading || isLoading}
        hasUploadedFile={!!fileInputLocation}
      />
      
      <textarea
        id="userInput"
        disabled={isLoading}
        className="chat-textarea"
        value={userInput}
        onChange={(e) => handleUserInputChange(e.target.value)}
        onKeyDown={handleKeyDown}
        placeholder="Type a message or select a file..."
      />
      
      <button
        id="submitButton"
        className="submit-button"
        type="button"
        onClick={handleSubmit}
        disabled={isLoading || (!userInput.trim() && !fileInputLocation)}
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
