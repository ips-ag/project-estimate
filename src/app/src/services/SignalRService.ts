import * as signalR from "@microsoft/signalr";
import { config } from "../config/config";
import { LogLevel } from "../types";

export default class SignalRService {
  private connection: signalR.HubConnection;
  private messageHandler: (assistant: string, message: string, logLevel: LogLevel) => void = () => {};
  private connectionIdCallback: (connectionId: string) => void = () => {};
  private isInitialized = false;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(config.apiUrl + "/hub")
      .withAutomaticReconnect([0, 2000, 10000, 30000, 30000, 30000, 30000, 30000])
      .build();
  }

  public initialize(
    onMessageReceived: (assistant: string, message: string, logLevel: LogLevel) => void,
    onConnectionIdReceived: (connectionId: string) => void
  ): void {
    if (this.isInitialized) return;

    this.messageHandler = onMessageReceived;
    this.connectionIdCallback = onConnectionIdReceived;

    this.connection.on("askQuestion", async (assistant: string, question: string) => {
      return "Doesn't matter";
    });
    this.connection.on("receiveMessage", (assistant: string, message: string, logLevel: LogLevel = LogLevel.Info) => {
      this.messageHandler(assistant, message, logLevel);
    });

    this.connection
      .start()
      .then(() => {
        if (this.connection.connectionId) {
          console.log("Connected to SignalR with connection ID:", this.connection.connectionId);
          this.connectionIdCallback(this.connection.connectionId);
        }
      })
      .catch((err) => {
        console.error("SignalR Connection Error:", err);
      });

    this.connection.onreconnected((connectionId) => {
      if (connectionId) {
        this.connectionIdCallback(connectionId);
        console.log("Reconnected to SignalR with connection ID:", connectionId);
      }
    });

    this.isInitialized = true;
    console.log("SignalR service initialized");
  }

  public getConnectionId(): string | null {
    return this.connection.connectionId;
  }
}
