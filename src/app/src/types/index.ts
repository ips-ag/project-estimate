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

export type FileUploadResponse = {
  location: string;
  errorMessage?: string;
};
