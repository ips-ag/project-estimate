import React from "react";
import MessageItem from "./MessageItem";
import { Message, MessageTypeModel } from "../../types";
import "./MessageList.css";

type MessageListProps = {
  messages: Message[];
  showReasoning: boolean;
};

export default function MessageList({ messages, showReasoning }: MessageListProps) {
  const filteredMessages = messages.filter((message) => {
    if (showReasoning) return true;
    return message.type !== MessageTypeModel.Reasoning;
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
