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
  const [showReasoning, setShowReasoning] = useState(false);
  const signalRServiceRef = useRef<SignalRService>(new SignalRService());

  useEffect(() => {
    const handleMessageReceived = (
      assistant: string,
      message: string,
      type: MessageTypeModel,
      final: boolean
    ): void => {
      setMessages((prevMessages) => [...prevMessages, { sender: assistant, text: message, type: type, final: final }]);
    };

    const handleUserInputRequested = (): string | null => {
      // TODO: Request user input for questions
      let response = "Doesn't matter";
      let userMessage: Message = {
        sender: "User",
        text: response,
        type: MessageTypeModel.Message,
        final: false,
      };
      setMessages((prevMessages) => [...prevMessages, userMessage]);
      return response;
    };

    const handleConnectionIdReceived = (connectionId: string) => {
      setSignalrConnectionId(connectionId);
    };

    signalRServiceRef.current.initialize(handleMessageReceived, handleUserInputRequested, handleConnectionIdReceived);
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
      const request = {
        connectionId: signalrConnectionId,
        input: message,
        fileInput: fileInputLocation,
      };
      setIsLoading(true);
      const data = await ApiService.completeConversation(request);
      setFileInputLocation(undefined);
      if (data.output !== undefined) {
        const assistantMessage: string = data.output;
        setMessages((prevMessages) => [
          ...prevMessages,
          { sender: "Assistant", text: assistantMessage, type: MessageTypeModel.Message, final: true },
        ]);
      }
    } catch (error) {
      console.error("Error during conversation:", error);
    } finally {
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
        onFileSelected={handleFileUpload}
        onSend={handleSendMessage}
      />
    </div>
  );
}
