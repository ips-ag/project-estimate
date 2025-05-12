import React, { useState, useEffect, useRef } from "react";
import sendIcon from "./assets/send.svg";
import spinnerIcon from "./assets/spinner.svg";
import logo from "./assets/logo.png";
import "./App.css";

// Import types
import { Message } from "./types";

// Import components
import Header from "./components/layout/Header";
import MessageList from "./components/chat/MessageList";
import ChatInput from "./components/chat/ChatInput";

// Import services
import { SignalRService } from "./services/SignalRService";
import { ApiService } from "./services/ApiService";

export default function App() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [userInput, setUserInput] = useState("");
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
  
  // Handle form submission
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!userInput.trim() && !fileInputLocation) return;

    // Add user message to the chat
    const newMessages: Message[] = [...messages, { sender: "User", text: userInput }];
    setMessages(newMessages);

    try {
      const request = {
        connectionId: signalrConnectionId,
        input: userInput,
        fileInput: fileInputLocation,
      };
      
      setIsLoading(true);
      setUserInput("");
      setFileInputLocation(undefined);
      
      const data = await ApiService.sendConversation(request);
      
      if (data.output !== undefined) {
        const message: string = data.output;
        setMessages((prevMessages) => [...prevMessages, { sender: "Assistant", text: message }]);
      }
    } catch (error) {
      console.error("Error during conversation:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="app-container">
      <Header logoSrc={logo} />
      
      <MessageList messages={messages} />
      
      <ChatInput
        userInput={userInput}
        isLoading={isLoading}
        isUploading={isUploading}
        fileInputLocation={fileInputLocation}
        onUserInputChange={setUserInput}
        onSubmit={handleSubmit}
        onFileUpload={(location) => {
          setIsUploading(false);
          setFileInputLocation(location);
        }}
        sendIcon={sendIcon}
        spinnerIcon={spinnerIcon}
      />
    </div>
  );
}
