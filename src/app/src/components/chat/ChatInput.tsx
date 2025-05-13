import { useState } from "react";
import type { KeyboardEvent } from "react";
import FileUploadButton from "./FileUploadButton";
import ApiService from "../../services/ApiService";
import sendIcon from "../../assets/send.svg";
import spinnerIcon from "../../assets/spinner.svg";
import "./ChatInput.css";

type ChatInputProps = {
  isLoading: boolean;
  onSend: (message: string, fileLocation?: string) => void;
};

export default function ChatInput({
  isLoading,
  onSend
}: ChatInputProps) {
  // Manage state internally
  const [userInput, setUserInput] = useState("");
  const [isUploading, setIsUploading] = useState(false);
  const [fileInputLocation, setFileInputLocation] = useState<string | undefined>(undefined);

  const handleUserInputChange = (input: string) => {
    setUserInput(input);
  };

  const handleFileSelected = async (file: File) => {
    try {
      setIsUploading(true);
      setFileInputLocation(undefined); // Reset any previous uploads
      const data = await ApiService.uploadFile(file);
      
      if (!data.errorMessage && data.location) {
        setFileInputLocation(data.location);
      } else {
        console.error("File upload failed:", data.errorMessage);
      }
    } catch (error) {
      console.error("Error uploading file:", error);
    } finally {
      setIsUploading(false);
    }
  };

  const handleSubmit = () => {
    if (!userInput.trim() && !fileInputLocation) return;
    
    // Send the message to parent component
    onSend(userInput, fileInputLocation);
    
    // Reset state after sending
    setUserInput("");
    setFileInputLocation(undefined);
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
        onFileSelected={handleFileSelected}
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
