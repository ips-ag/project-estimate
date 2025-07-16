import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";
import { Message, MessageTypeModel } from "../../types";
import "./MessageItem.css";

type MessageItemProps = {
  message: Message;
};

export default function MessageItem({ message }: MessageItemProps) {
  const { sender, text, type } = message;
  const isReasoning = type === MessageTypeModel.Reasoning;
  const messageClass = `message-item ${isReasoning ? "message-reasoning" : ""}`;

  return (
    <div className={messageClass}>
      <strong>{sender}</strong>{" "}
      {text.includes("\n") ? <ReactMarkdown remarkPlugins={[remarkGfm]}>{text}</ReactMarkdown> : text}
    </div>
  );
}
