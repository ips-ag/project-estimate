import * as signalR from "@microsoft/signalr";
import { config } from "../config/config";
import { MessageTypeModel } from "../types";

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
    onConnectionIdReceived: (connectionId: string) => void,
    onUserInputTimeout?: () => void
  ): void {
    if (this.isInitialized) return;

    this.messageHandler = onMessageReceived;
    this.userInputRequestHandler = onUserInputRequested;
    this.connectionIdCallback = onConnectionIdReceived;
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
          this.userInputTimeoutHandler(); // Notify UI about timeout
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

  private messageHandler: (assistant: string, message: string, type: MessageTypeModel, final: boolean) => void =
    () => {};

  private userInputRequestHandler: () => void = () => {};

  private userInputTimeoutHandler: () => void = () => {};

  private connectionIdCallback: (connectionId: string) => void = () => {};
}
