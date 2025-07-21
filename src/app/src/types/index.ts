export enum MessageTypeModel {
  Message = 0,
  Reasoning = 1
}

export type Message = {
  sender: string;
  text: string;
  type: MessageTypeModel;
  final: boolean;
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
