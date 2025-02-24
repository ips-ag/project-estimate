import React, { useState } from "react";

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

    // Add user message
    const newMessages: Message[] = [...messages, { sender: "user", text: userInput }];
    setMessages(newMessages);

    // POST request
    try {
      const response = await fetch("http://localhost:7071/api/conversation", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ input: userInput } as ConversationRequest),
      });
      const data: ConversationResponse = await response.json();
      if (!!data.output) {
        // Add assistant message
        setMessages([...newMessages, { sender: "assistant", text: data.output }]);
      }
    } catch {
      // Handle error if needed
    }

    setUserInput("");
  }

  return (
    <div style={{ display: "flex", flexDirection: "column", height: "100vh" }}>
      <div style={{ flexGrow: 1, overflowY: "auto", padding: "1rem" }}>
        {messages.map((msg, i) => (
          <div key={i} style={{ margin: "0.5rem 0" }}>
            <strong>{msg.sender}:</strong> {msg.text}
          </div>
        ))}
      </div>
      <form onSubmit={handleSubmit} style={{ display: "flex", margin: "1rem" }}>
        <input style={{ flexGrow: 1 }} type="text" value={userInput} onChange={(e) => setUserInput(e.target.value)}/>
        <button type="submit">Send</button>
      </form>
    </div>
  );
}
