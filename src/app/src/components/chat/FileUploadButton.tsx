import React from "react";
import "./FileUploadButton.css";

type FileUploadButtonProps = {
  onFileSelect: (e: React.ChangeEvent<HTMLInputElement>) => void;
  isUploading: boolean;
  isDisabled: boolean;
  hasUploadedFile: boolean;
  fileInputRef: React.RefObject<HTMLInputElement | null>;
};

const FileUploadButton: React.FC<FileUploadButtonProps> = ({
  onFileSelect,
  isUploading,
  isDisabled,
  hasUploadedFile,
  fileInputRef,
}) => {
  return (
    <>
      <input
        type="file"
        accept=".txt,.html,.md,.markdown,.pdf,.jpg,.jpeg,.png,.bmp,.tiff,.heif,.docx,.xlsx,.pptx"
        className="file-input"
        onChange={onFileSelect}
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
};

export default FileUploadButton;
