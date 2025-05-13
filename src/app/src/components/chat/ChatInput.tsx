import React, { useRef } from "react";
import FileUploadButton from "./FileUploadButton";
import ApiService from "../../services/ApiService";
import sendIcon from "../../assets/send.svg";
import spinnerIcon from "../../assets/spinner.svg";
import "./ChatInput.css";

type ChatInputProps = {
  userInput: string;
  isLoading: boolean;
  isUploading: boolean;
  fileInputLocation: string | undefined;
  onUserInputChange: (input: string) => void;
  onSubmit: (e: React.FormEvent) => void;
  onFileUpload: (location: string | undefined) => void;
};

export default function ChatInput({
  userInput,
  isLoading,
  isUploading,
  fileInputLocation,
  onUserInputChange,
  onSubmit,
  onFileUpload
}: ChatInputProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    try {
      onFileUpload(undefined); // Reset any previous uploads
      const data = await ApiService.uploadFile(file);
      
      if (!data.errorMessage && data.location) {
        onFileUpload(data.location);
      } else {
        console.error("File upload failed:", data.errorMessage);
      }
    } catch (error) {
      console.error("Error uploading file:", error);
    } finally {
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  };

  return (
    <form
      onSubmit={onSubmit}
      className="chat-form"
    >
      <FileUploadButton
        onFileSelect={handleFileUpload}
        isUploading={isUploading}
        isDisabled={isUploading || isLoading}
        hasUploadedFile={!!fileInputLocation}
        fileInputRef={fileInputRef}
      />
      
      <textarea
        id="userInput"
        disabled={isLoading}
        className="chat-textarea"
        value={userInput}
        onChange={(e) => onUserInputChange(e.target.value)}
      />
      
      <button
        id="submitButton"
        className="submit-button"
        type="submit"
      >
        {isLoading ? (
          <img src={spinnerIcon} alt="Loading..." className="button-icon spinner" />
        ) : (
          <img src={sendIcon} alt="Send" className="button-icon" />
        )}
      </button>
    </form>
  );
}
