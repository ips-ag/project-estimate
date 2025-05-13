import { useState, useEffect, useRef } from "react";
import "./App.css";

// Import types
import { Message } from "./types";

// Import components
import Header from "./components/layout/Header";
import MessageList from "./components/chat/MessageList";
import ChatInput from "./components/chat/ChatInput";

// Import services
import SignalRService from "./services/SignalRService";
import ApiService from "./services/ApiService";

export default function App() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [fileInputLocation, setFileInputLocation] = useState<string | undefined>(undefined);
  const [signalrConnectionId, setSignalrConnectionId] = useState<string | undefined>("");
  
  // Create SignalR service instance once
  const signalRServiceRef = useRef<SignalRService>(new SignalRService());
  
  // Initialize SignalR connection
  useEffect(() => {
    const handleMessageReceived = (assistant: string, message: string) => {
      setMessages((prevMessages) => [...prevMessages, { sender: assistant, text: message }]);
    };
    
    const handleConnectionIdReceived = (connectionId: string) => {
      setSignalrConnectionId(connectionId);
    };
    
    signalRServiceRef.current.initialize(handleMessageReceived, handleConnectionIdReceived);
  }, []);
  
  // Handle file uploads
  const handleFileUpload = async (file: File) => {
    try {
      setIsUploading(true);
      setFileInputLocation(undefined); // Reset any previous uploads
      
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
  
  // Handle sending messages and files
  const handleSendMessage = async (message: string) => {
    // Don't send if both message and fileInputLocation are empty
    if (!message.trim() && !fileInputLocation) return;

    // Add user message to the chat
    setMessages(prevMessages => [...prevMessages, { sender: "User", text: message }]);

    try {
      const request = {
        connectionId: signalrConnectionId,
        input: message,
        fileInput: fileInputLocation,
      };
      
      setIsLoading(true);
      
      const data = await ApiService.sendConversation(request);
      
      // Reset file input after sending
      setFileInputLocation(undefined);
      
      if (data.output !== undefined) {
        const assistantMessage: string = data.output;
        setMessages((prevMessages) => [...prevMessages, { sender: "Assistant", text: assistantMessage }]);
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
      
      <MessageList messages={messages} />
      
      <ChatInput
        isLoading={isLoading}
        isUploading={isUploading}
        fileInputLocation={fileInputLocation}
        onFileSelected={handleFileUpload}
        onSend={handleSendMessage}
      />
    </div>
  );
}
