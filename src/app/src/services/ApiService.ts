import { config } from "../config/config";
import { ConversationRequest, ConversationResponse, FileUploadResponse } from "../types";

export default class ApiService {
  public static async uploadFile(file: File): Promise<FileUploadResponse> {
    try {
      const formData = new FormData();
      formData.append("file", file, file.name);
      const response = await fetch(config.apiUrl + "/file", {
        method: "POST",
        body: formData,
      });
      return await response.json();
    } catch (error) {
      console.error("Error uploading file:", error);
      return {
        location: "",
        errorMessage: "Failed to upload file",
      };
    }
  }

  public static async completeConversation(request: ConversationRequest): Promise<ConversationResponse> {
    try {
      const response = await fetch(config.apiUrl + "/conversation", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(request),
      });
      return await response.json();
    } catch (error) {
      console.error("Error sending conversation:", error);
      return {
        responseRequired: false,
      };
    }
  }
}
