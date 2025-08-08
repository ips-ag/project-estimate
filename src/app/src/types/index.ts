export enum MessageTypeModel {
  Message = 0,
  Reasoning = 1
}

export enum ConnectionState {
  Connected = 'connected',
  Connecting = 'connecting',
  Disconnected = 'disconnected'
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
