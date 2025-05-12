// Types used across the application

export type Message = {
  sender: string;
  text: string;
};

export type ConversationRequest = {
  connectionId?: string;
  input?: string;
  fileInput?: string;
};

export type ConversationResponse = {
  output?: string;
  responseRequired: boolean;
};

export type FileUploadResponse = {
  location: string;
  errorMessage?: string;
};
