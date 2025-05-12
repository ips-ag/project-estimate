import React from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { Message } from "../../types";
import "./MessageItem.css";

type MessageItemProps = {
  message: Message;
};

const MessageItem: React.FC<MessageItemProps> = ({ message }) => {
  const { sender, text } = message;

  return (
    <div className="message-item">
      <strong>{sender}</strong>{" "}
      {text.includes("\n") ? (
        <ReactMarkdown remarkPlugins={[remarkGfm]}>{text}</ReactMarkdown>
      ) : (
        text
      )}
    </div>
  );
};

export default MessageItem;
