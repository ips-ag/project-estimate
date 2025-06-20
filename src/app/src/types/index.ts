export enum LogLevel {
  Trace = 0,
  Debug = 1,
  Info = 2,
  Warning = 3,
  Error = 4,
  Critical = 5
}

export type Message = {
  sender: string;
  text: string;
  logLevel?: LogLevel;
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
