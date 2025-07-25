import { useEffect, useRef, useState } from "react";
import { Message, MessageTypeModel } from "./types";
import Header from "./components/layout/Header";
import MessageList from "./components/chat/MessageList";
import ChatInput from "./components/chat/ChatInput";
import ReasoningToggle from "./components/chat/ReasoningToggle";
import SignalRService from "./services/SignalRService";
import ApiService from "./services/ApiService";
import "./App.css";

export default function App() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [fileInputLocation, setFileInputLocation] = useState<string | undefined>(undefined);
  const [signalrConnectionId, setSignalrConnectionId] = useState<string | undefined>("");
  const [showReasoning, setShowReasoning] = useState(() => {
    const saved = localStorage.getItem("showReasoning");
    return saved === "true";
  });
  const [isWaitingForUserInput, setIsWaitingForUserInput] = useState(false);
  const signalRServiceRef = useRef<SignalRService>(new SignalRService());

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
    };

    const handleUserInputTimeout = (): void => {
      setMessages((prevMessages) => [
        ...prevMessages,
        { sender: "User", text: "No answer provided", type: MessageTypeModel.Message, final: false },
      ]);
      setIsWaitingForUserInput(false);
      setIsLoading(true);
    };

    const handleConnectionIdReceived = (connectionId: string) => {
      setSignalrConnectionId(connectionId);
    };

    signalRServiceRef.current.initialize(
      handleMessageReceived,
      handleUserInputRequested,
      handleConnectionIdReceived,
      handleUserInputTimeout
    );
  }, []);

  const handleFileUpload = async (file: File) => {
    try {
      setIsUploading(true);
      setFileInputLocation(undefined);
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
      const request = {
        connectionId: signalrConnectionId,
        input: message,
        fileInput: fileInputLocation,
      };
      setIsLoading(true);
      await ApiService.completeConversation(request);
      setFileInputLocation(undefined);
    } catch (error) {
      console.error("Error during conversation:", error);
      setIsLoading(false);
    }
  };

  return (
    <div className="app-container">
      <Header />
      <MessageList messages={messages} showReasoning={showReasoning} />
      <ReasoningToggle showReasoning={showReasoning} onReasoningToggle={setShowReasoning} />
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
