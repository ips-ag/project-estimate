import React, { useState, useRef } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import sendIcon from "./assets/send.svg";
import spinnerIcon from "./assets/spinner.svg";
import logo from "./assets/logo.png";
import { config } from "./config/config.ts";
import * as signalR from "@microsoft/signalr";
import "./App.css";

type Message = {
  sender: string;
  text: string;
};

type ConversationRequest = {
  connectionId?: string;
  input?: string;
  fileInput?: string;
};

type ConversationResponse = {
  output?: string;
  responseRequired: boolean;
};

type FileUploadResponse = {
  location: string;
  errorMessage?: string;
};

export default function App() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [userInput, setUserInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [isSignalrInitialized, setIsSignalrInitialized] = useState(false);
  const [signalrConnectionId, setSignalrConnectionId] = useState<string | undefined>("");
  const [isUploading, setIsUploading] = useState(false);
  const [fileInputLocation, setFileInputLocation] = useState<string | undefined>(undefined);
  const fileInputRef = useRef<HTMLInputElement>(null);

  if (!isSignalrInitialized) {
    console.log("Registering SignalR handlers");
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(config.apiUrl + "/hub")
      .withAutomaticReconnect([0, 2000, 10000, 30000, 30000, 30000, 30000, 30000])
      .build();
    connection
      .start()
      .then(() => {
        if (connection.connectionId) {
          console.log("Connected to SignalR with connection ID:", connection.connectionId);
          setSignalrConnectionId(connection.connectionId);
        }
      })
      .catch((err) => {
        // TODO: signal connection error
        console.error(err);
      });

    connection.onreconnected((connectionId) => {
      if (connectionId) {
        setSignalrConnectionId(connectionId);
        console.log("Reconnected to SignalR with connection ID:", connectionId);
      }
    });

    connection.on("receiveMessage", (assistant: string, message: string) => {
      setMessages((prevMessages) => [...prevMessages, { sender: assistant, text: message }]);
    });
    setIsSignalrInitialized(true);
    console.log("Registered SignalR handlers");
  }

  async function handleFileUpload(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    if (!file) return;
    try {
      setIsUploading(true);
      const formData = new FormData();
      formData.append("file", file, file.name);
      const response = await fetch(config.apiUrl + "/file", {
        method: "POST",
        body: formData,
      });
      const data: FileUploadResponse = await response.json();
      if (!data.errorMessage && data.location) {
        setFileInputLocation(data.location);
      } else {
        console.error("File upload failed:", data.errorMessage);
      }
    } catch (error) {
      console.error("Error uploading file:", error);
    } finally {
      setIsUploading(false);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!userInput.trim() && !fileInputLocation) return;

    const newMessages: Message[] = [...messages, { sender: "User", text: userInput }];
    setMessages(newMessages);

    try {
      const request: ConversationRequest = {
        connectionId: signalrConnectionId,
        input: userInput,
        fileInput: fileInputLocation,
      };
      setIsLoading(true);
      setUserInput("");
      setFileInputLocation(undefined);
      const response = await fetch(config.apiUrl + "/conversation", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request),
      });
      const data: ConversationResponse = await response.json();
      if (data.output !== undefined) {
        const message: string = data.output;
        setMessages((prevMessages) => [...prevMessages, { sender: "Assistant", text: message }]);
      }
    } catch {
      // Handle error if needed
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <div
      style={{
        position: "relative",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        width: "100vw",
        height: "100vh",
        backgroundColor: "#e0f3ff",
      }}
    >
      <img
        src={logo}
        alt="logo"
        style={{
          position: "absolute",
          top: "10px",
          left: "10px",
          zIndex: 10,
          height: "70px",
          width: "auto",
        }}
      />
      <div
        style={{
          marginTop: "1rem",
          width: "60%",
          flexGrow: 1,
          overflowY: "auto",
          padding: "1rem",
          backgroundColor: "#fff",
          borderRadius: "12px",
          boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
          display: "flex",
          flexDirection: "column",
        }}
      >
        {messages.map((msg, i) => (
          <React.Fragment key={i}>
            <div style={{ margin: "0.5rem 0" }}>
              <strong>{msg.sender}</strong>{" "}
              {msg.text.includes("\n") ? (
                <ReactMarkdown remarkPlugins={[remarkGfm]}>{msg.text}</ReactMarkdown>
              ) : (
                msg.text
              )}
            </div>
            {i < messages.length - 1 && <hr style={{ width: "100%", border: "1px solid rgba(13, 13, 13, 0.05)" }} />}
          </React.Fragment>
        ))}
      </div>
      <form
        onSubmit={handleSubmit}
        style={{
          position: "relative",
          width: "70%",
          margin: "1rem",
          borderRadius: "12px",
        }}
      >
        <input
          type="file"
          accept=".txt,.html,.pdf,.jpg,.jpeg,.png,.bmp,.tiff,.heif,.docx,.xlsx,.pptx"
          style={{ display: "none" }}
          onChange={handleFileUpload}
          ref={fileInputRef}
          id="fileInput"
        />
        <button
          type="button"
          onClick={() => fileInputRef.current?.click()}
          disabled={isUploading || isLoading}
          style={{
            position: "absolute",
            left: "0.5rem",
            top: "50%",
            transform: "translateY(-50%)",
            zIndex: 10,
            border: "none",
            background: "none",
            cursor: "pointer",
            fontSize: "1.2rem",
            padding: "0.5rem",
            borderRadius: "4px",
            backgroundColor: "#f0f0f0",
          }}
          title="Upload file (text, image, document)"
        >
          {isUploading ? "⏳" : fileInputLocation ? "📄" : "📎"}
        </button>
        <textarea
          id="userInput"
          disabled={isLoading}
          style={{
            width: "100%",
            height: "6rem",
            borderRadius: "12px",
            fontSize: "1.4rem",
            paddingLeft: "2.5rem",
            paddingRight: "2rem",
            paddingTop: "0.5rem",
            resize: "none",
          }}
          value={userInput}
          onChange={(e) => setUserInput(e.target.value)}
        />
        <button
          id="submitButton"
          style={{
            position: "absolute",
            right: "-3rem",
            top: "50%",
            transform: "translateY(-50%)",
            borderRadius: "50%",
            width: "2rem",
            height: "2rem",
            padding: 0,
            transition: "none",
          }}
          type="submit"
        >
          {isLoading ? (
            <img src={spinnerIcon} alt="Loading..." style={{ width: "100%", height: "100%" }} className="spinner" />
          ) : (
            <img src={sendIcon} alt="Send" style={{ width: "100%", height: "100%" }} />
          )}
        </button>
      </form>
    </div>
  );
}
