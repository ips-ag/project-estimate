import React from "react";
import MessageItem from "./MessageItem";
import { Message } from "../../types";
import "./MessageList.css";

type MessageListProps = {
  messages: Message[];
};

export default function MessageList({ messages }: MessageListProps) {
  return (
    <div className="message-list">
      {messages.map((message, index) => (
        <React.Fragment key={index}>
          <MessageItem message={message} />
          {index < messages.length - 1 && <hr className="message-divider" />}
        </React.Fragment>
      ))}
    </div>
  );
}
