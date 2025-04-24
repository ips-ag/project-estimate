import React, { useState } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from 'remark-gfm'
import sendIcon from "./assets/send.svg";
import spinnerIcon from "./assets/spinner.svg";
import logo from "./assets/logo.png";
import { config } from "./config/config.ts";
import * as signalR from "@microsoft/signalr";
import './App.css';

type Message = {
  sender: string;
  text: string;
};

type ConversationRequest = {
  input?: string;
};

type ConversationResponse = {
  output?: string;
  responseRequired: boolean;
};

export default function App() {
  const [messages, setMessages] = useState<Message[]>([]);
  const [userInput, setUserInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  const connection = new signalR.HubConnectionBuilder()
    .withUrl(config.apiUrl + "/hub")
    .build();

  connection.on("receiveMessage", (assistant: string, message: string) => {
    setMessages((prevMessages) => [
      ...prevMessages,
      { sender: assistant, text: message },
    ]);
  });

  connection.start().catch((err) => {
    // TODO: signal connection error
    console.error(err);
  });

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!userInput.trim()) return;

    const newMessages: Message[] = [...messages, { sender: "User", text: userInput }];
    setMessages(newMessages);

    try {
      const request: ConversationRequest = { input: userInput };
      setIsLoading(true);
      setUserInput("");
      const response = await fetch(config.apiUrl + "/conversation", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request),
      });
      const data: ConversationResponse = await response.json();
      if (data.output !== undefined) {
        const message: string = data.output;
        setMessages(prevMessages => [
          ...prevMessages,
          { sender: "Assistant", text: message },
        ]);
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
          <div key={i} style={{ margin: "0.5rem 0" }}>
            <strong>{msg.sender}:</strong> <ReactMarkdown remarkPlugins={[remarkGfm]}>{msg.text}</ReactMarkdown>
          </div>
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
        <textarea
          id="userInput"
          disabled={isLoading}
          style={{
            width: "100%",
            height: "6rem",
            borderRadius: "12px",
            fontSize: "1.4rem",
            paddingLeft: "0.75rem",
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
            right: "0",
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
            <img
              src={spinnerIcon}
              alt="Loading..."
              style={{ width: "100%", height: "100%" }}
              className="spinner"
            />
          ) : (
            <img
              src={sendIcon}
              alt="Send"
              style={{ width: "100%", height: "100%" }}
            />
          )}
        </button>
      </form>
    </div>
  );
}
