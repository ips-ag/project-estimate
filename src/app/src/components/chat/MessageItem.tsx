import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { Message } from "../../types";
import "./MessageItem.css";

type MessageItemProps = {
  message: Message;
};

export default function MessageItem({ message }: MessageItemProps) {
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
}
