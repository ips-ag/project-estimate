import { useEffect, useRef, useState } from "react";
import { Message, MessageTypeModel } from "./types";
import Header from "./components/layout/Header";
import MessageList from "./components/chat/MessageList";
import ChatInput from "./components/chat/ChatInput";
import ReasoningToggle from "./components/chat/ReasoningToggle";
import ConnectionStatus from "./components/chat/ConnectionStatus";
import SignalRService from "./services/SignalRService";
import ApiService from "./services/ApiService";
import "./App.css";

export default function App() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [fileInputLocation, setFileInputLocation] = useState<string | null>(null);
  const [isSignalRConnected, setIsSignalRConnected] = useState(false);
  const [showReasoning, setShowReasoning] = useState(() => {
    const saved = localStorage.getItem("showReasoning");
    return saved === "true";
  });
  const [isWaitingForUserInput, setIsWaitingForUserInput] = useState(false);
  const signalRServiceRef = useRef<SignalRService>(new SignalRService());

  const focusUserInput = (delay: number = 0): void => {
    setTimeout(() => {
      const inputElement = document.getElementById("userInput") as HTMLTextAreaElement;
      if (inputElement) {
        inputElement.focus();
      }
    }, delay);
  };

  useEffect(() => {
    focusUserInput();
  }, []);

  useEffect(() => {
    localStorage.setItem("showReasoning", showReasoning ? "true" : "false");
  }, [showReasoning]);

  useEffect(() => {
    const handleMessageReceived = (
      assistant: string,
      message: string,
      type: MessageTypeModel,
      final: boolean
    ): void => {
      setMessages((prevMessages) => [...prevMessages, { sender: assistant, text: message, type: type, final: final }]);
      if (final) {
        setIsLoading(false);
      }
    };

    const handleUserInputRequested = (): void => {
      setIsLoading(false);
      setIsWaitingForUserInput(true);
      focusUserInput(100);
    };

    const handleUserInputTimeout = (): void => {
      setMessages((prevMessages) => [
        ...prevMessages,
        { sender: "User", text: "No answer provided", type: MessageTypeModel.Message, final: false },
      ]);
      setIsWaitingForUserInput(false);
      setIsLoading(true);
    };

    const handleConnectionStateChanged = (isConnected: boolean): void => {
      setIsSignalRConnected(isConnected);
    };

    signalRServiceRef.current.initialize(
      handleMessageReceived,
      handleUserInputRequested,
      handleConnectionStateChanged,
      handleUserInputTimeout
    );
  }, []);

  const handleFileUpload = async (file: File) => {
    try {
      setIsUploading(true);
      setFileInputLocation(null);
      const data = await ApiService.uploadFile(file);
      if (!data.errorMessage && data.location) {
        setFileInputLocation(data.location);
      } else {
        console.error("File upload failed:", data.errorMessage);
      }
    } catch (error) {
      console.error("Error uploading file:", error);
    } finally {
      setIsUploading(false);
    }
  };
  const handleSendMessage = async (message: string) => {
    if (!message.trim() && !fileInputLocation) return;
    setMessages((prevMessages) => [
      ...prevMessages,
      { sender: "User", text: message, type: MessageTypeModel.Message, final: false },
    ]);

    try {
      if (isWaitingForUserInput) {
        setIsWaitingForUserInput(false);
        setIsLoading(true);
        signalRServiceRef.current.provideUserInput(message);
        return;
      }

      setIsLoading(true);
      await signalRServiceRef.current.sendMessage(message, fileInputLocation);
      setFileInputLocation(null);
    } catch (error) {
      console.error("Error during conversation:", error);
      setIsLoading(false);
    }
  };

  return (
    <div className="app-container">
      <Header />
      <MessageList messages={messages} showReasoning={showReasoning} />
      <div className="action-bar">
        <ReasoningToggle showReasoning={showReasoning} onReasoningToggle={setShowReasoning} />
        <ConnectionStatus isConnected={isSignalRConnected} />
      </div>
      <ChatInput
        isLoading={isLoading}
        isUploading={isUploading}
        hasInputFile={!!fileInputLocation}
        isWaitingForUserInput={isWaitingForUserInput}
        onFileSelected={handleFileUpload}
        onSend={handleSendMessage}
      />
    </div>
  );
}
