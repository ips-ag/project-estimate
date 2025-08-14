import { config } from "../config/config";
import { FileUploadResponse } from "../types";
import { getAccessToken } from "../auth/tokenService";

export default class ApiService {
  public static async uploadFile(file: File): Promise<FileUploadResponse> {
    try {
      const token = await getAccessToken();
      const headers: HeadersInit = {};
      if (token) {
        headers["Authorization"] = `Bearer ${token}`;
      }
      const formData = new FormData();
      formData.append("file", file, file.name);
      const response = await fetch(config.apiUrl + "/file", {
        method: "POST",
        headers: headers,
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
}
