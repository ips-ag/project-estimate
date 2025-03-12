import React, { useState } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from 'remark-gfm'
import sendIcon from "./assets/send.svg";
import logo from "./assets/logo.png";
import { config } from "./config/config.ts";

type Message = {
  sender: "user" | "assistant";
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

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!userInput.trim()) return;

    const newMessages: Message[] = [...messages, { sender: "user", text: userInput }];
    setMessages(newMessages);

    try {
      const response = await fetch(config.apiUrl + "/conversation", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ input: userInput } as ConversationRequest),
      });
      const data: ConversationResponse = await response.json();
      if (!!data.output) {
        setMessages([...newMessages, { sender: "assistant", text: data.output }]);
      }
    } catch {
      // Handle error if needed
    }

    setUserInput("");
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
        <input
          style={{
            width: "100%",
            height: "3rem",
            borderRadius: "12px",
            fontSize: "1.4rem",
            paddingLeft: "0.75rem",
          }}
          type="text"
          value={userInput}
          onChange={(e) => setUserInput(e.target.value)}
        />
        <button
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
          <img
            src={sendIcon}
            alt="send"
            style={{
              width: "100%",
              height: "100%",
            }}
          />
        </button>
      </form>
    </div>
  );
}
