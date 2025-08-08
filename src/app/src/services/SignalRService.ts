import * as signalR from "@microsoft/signalr";
import { config } from "../config/config";
import { MessageTypeModel, ConnectionState } from "../types";

export default class SignalRService {
  private connection: signalR.HubConnection;
  private isInitialized = false;
  private userInputResolver: ((value: string) => void) | null = null;
  private userInputPromise: Promise<string> | null = null;
  private userInputTimeout: NodeJS.Timeout | null = null;

  constructor() {
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(config.apiUrl + "/hub")
      .withAutomaticReconnect([0, 2000, 10000, 30000, 30000, 30000, 30000, 30000])
      .build();
  }

  public initialize(
    onMessageReceived: (assistant: string, message: string, type: MessageTypeModel, final: boolean) => void,
    onUserInputRequested: () => void,
    onConnectionStateChanged?: (status: ConnectionState) => void,
    onUserInputTimeout?: () => void
  ): void {
    if (this.isInitialized) return;

    this.messageHandler = onMessageReceived;
    this.userInputRequestHandler = onUserInputRequested;
    this.connectionStateChangedHandler = onConnectionStateChanged || (() => {});
    this.userInputTimeoutHandler = onUserInputTimeout || (() => {});

    this.connection.on(
      "receiveMessage",
      (assistant: string, message: string, type: MessageTypeModel, final: boolean) => {
        this.messageHandler(assistant, message, type, final);
      }
    );

    this.connection.on("getUserInput", async (): Promise<string | null> => {
      this.userInputPromise = new Promise<string>((resolve) => {
        this.userInputResolver = resolve;
      });
      this.userInputTimeout = setTimeout(() => {
        if (this.userInputResolver) {
          this.userInputTimeoutHandler();
          this.userInputResolver("No answer provided");
          this.userInputResolver = null;
          this.userInputPromise = null;
          this.userInputTimeout = null;
        }
      }, 30000);
      this.userInputRequestHandler();
      try {
        return await this.userInputPromise;
      } catch (error) {
        console.error("Error getting user input:", error);
        return null;
      }
    });

    this.connectionStateChangedHandler(ConnectionState.Connecting);

    this.connection
      .start()
      .then(() => {
        if (this.connection.connectionId) {
          console.log("Connected to SignalR with connection ID:", this.connection.connectionId);
          this.connectionStateChangedHandler(ConnectionState.Connected);
        }
      })
      .catch((err) => {
        console.error("SignalR connection error:", err);
        this.connectionStateChangedHandler(ConnectionState.Disconnected);
      });

    this.connection.onreconnected((connectionId) => {
      if (connectionId) {
        console.log("Reconnected to SignalR with connection ID:", connectionId);
        this.connectionStateChangedHandler(ConnectionState.Connected);
      }
    });

    this.connection.onreconnecting((error) => {
      this.connectionStateChangedHandler(ConnectionState.Connecting);
    });

    this.connection.onclose(() => {
      this.connectionStateChangedHandler(ConnectionState.Disconnected);
    });

    this.isInitialized = true;
    console.log("SignalR service initialized");
  }

  public provideUserInput(input: string): void {
    if (this.userInputResolver) {
      if (this.userInputTimeout) {
        clearTimeout(this.userInputTimeout);
        this.userInputTimeout = null;
      }
      this.userInputResolver(input);
      this.userInputResolver = null;
      this.userInputPromise = null;
    }
  }

  public async sendMessage(prompt: string | null, fileLocation: string | null): Promise<void> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      throw new Error("SignalR connection is not established.");
    }
    try {
      await this.connection.invoke("sendMessage", prompt, fileLocation);
    } catch (error) {
      console.error("Error sending message via SignalR:", error);
      throw error;
    }
  }

  private messageHandler: (assistant: string, message: string, type: MessageTypeModel, final: boolean) => void =
    () => {};

  private userInputRequestHandler: () => void = () => {};

  private userInputTimeoutHandler: () => void = () => {};

  private connectionStateChangedHandler: (status: ConnectionState) => void = () => {};
}
