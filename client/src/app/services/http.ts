import { tokenManager } from './tokenManager';

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

export async function apiFetch(url: string, options: RequestInit = {}) {
  const token = tokenManager.getAccessToken();

  const headers = new Headers(options.headers || {});
  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }
  if (!headers.has('Content-Type') && !(options.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json');
  }

  options.headers = headers;

  let response = await fetch(url, options);

  // Não tenta renovar token se for a rota de login, refresh ou logout
  const isAuthRoute = url.includes('/Auth/login') || url.includes('/Auth/refresh') || url.includes('/Auth/logout');

  if (response.status === 401 && !isAuthRoute) {
    const newToken = await tokenManager.refreshAccessToken();
    if (newToken) {
      headers.set('Authorization', `Bearer ${newToken}`);
      options.headers = headers;
      response = await fetch(url, options);
    }
  }

  return response;
}
