import React from "react";
import MessageItem from "./MessageItem";
import { Message, LogLevel } from "../../types";
import "./MessageList.css";

type MessageListProps = {
  messages: Message[];
  showDebugMessages: boolean;
};

export default function MessageList({ messages, showDebugMessages }: MessageListProps) {
  const filteredMessages = messages.filter((message) => {
    if (showDebugMessages) return true;
    const logLevel = message.logLevel ?? LogLevel.Info;
    return logLevel > LogLevel.Debug;
  });

  return (
    <div className="message-list">
      {filteredMessages.map((message, index) => (
        <React.Fragment key={index}>
          <MessageItem message={message} />
          {index < filteredMessages.length - 1 && <hr className="message-divider" />}
        </React.Fragment>
      ))}
    </div>
  );
}
