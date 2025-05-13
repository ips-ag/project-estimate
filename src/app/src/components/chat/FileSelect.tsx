import { useRef } from "react";
import type { ChangeEvent } from "react";
import "./FileSelect.css";

type FileSelectProps = {
  onFileSelected: (file: File) => void;
  isUploading: boolean;
  isDisabled: boolean;
  hasUploadedFile: boolean;
};

export default function FileSelect({
  onFileSelected,
  isUploading,
  isDisabled,
  hasUploadedFile,
}: FileSelectProps) {
  // Internal file input reference
  const fileInputRef = useRef<HTMLInputElement>(null);
  
  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      onFileSelected(file);
    }
    
    // Reset input to allow selecting the same file again
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };
  
  return (
    <>
      <input
        type="file"
        accept=".txt,.html,.md,.markdown,.pdf,.jpg,.jpeg,.png,.bmp,.tiff,.heif,.docx,.xlsx,.pptx"
        className="file-input"
        onChange={handleFileChange}
        ref={fileInputRef}
        id="fileInput"
      />
      <button
        type="button"
        onClick={() => fileInputRef.current?.click()}
        disabled={isDisabled}
        className="upload-button"
        title="Upload file (text, image, document)"
      >
        {isUploading ? "⏳" : hasUploadedFile ? "📄" : "📎"}
      </button>
    </>
  );
}
