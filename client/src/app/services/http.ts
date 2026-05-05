export const BASE_URL = import.meta.env.VITE_BACKEND_BASE_URL || 'http://localhost:5262/api';

export class ApiError extends Error {
  status?: number;
  data?: any;

  constructor(message: string, status?: number, data?: any) {
    super(message);
    this.status = status;
    this.data = data;
    this.name = 'ApiError';
  }
}

export async function handleResponse(response: Response, defaultErrorMessage: string) {
  if (!response.ok) {
    const errorText = await response.text();
    let errorMessage = defaultErrorMessage;
    let errJson = null;
    
    try {
      errJson = JSON.parse(errorText);
      errorMessage = errJson.detail || errJson.title || errorMessage;
    } catch(e) {
      errorMessage = errorText || errorMessage;
    }
    throw new ApiError(errorMessage, response.status, errJson);
  }
  
  const text = await response.text();
  if (!text) return {};
  
  try {
      return JSON.parse(text);
  } catch (e) {
      return text;
  }
}
