import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { Message, LogLevel } from "../../types";
import "./MessageItem.css";

type MessageItemProps = {
  message: Message;
};

export default function MessageItem({ message }: MessageItemProps) {
  const { sender, text, logLevel } = message;
  
  // Check if the message should be grayed out (Debug or Trace levels)
  // Default to Info level if logLevel is undefined for backward compatibility
  const isDebugLevel = logLevel === LogLevel.Debug || logLevel === LogLevel.Trace;
  const messageClass = `message-item ${isDebugLevel ? 'message-debug' : ''}`;

  return (
    <div className={messageClass}>
      <strong>{sender}</strong>{" "}
      {text.includes("\n") ? <ReactMarkdown remarkPlugins={[remarkGfm]}>{text}</ReactMarkdown> : text}
    </div>
  );
}
