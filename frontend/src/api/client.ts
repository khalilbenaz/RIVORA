import axios, { type AxiosError, type InternalAxiosRequestConfig } from 'axios';
import { useAuthStore } from '../store/authStore';

interface RetryConfig extends InternalAxiosRequestConfig {
  __retryCount?: number;
}

const MAX_RETRIES = 3;
const BASE_DELAY_MS = 1000;

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const config = error.config as RetryConfig | undefined;
    const status = error.response?.status;

    // Retry logic for 429 and 503
    if (config && (status === 429 || status === 503)) {
      const retryCount = config.__retryCount ?? 0;

      if (retryCount < MAX_RETRIES) {
        config.__retryCount = retryCount + 1;

        // Respect Retry-After header for 429, otherwise use exponential backoff
        let delay = BASE_DELAY_MS * Math.pow(2, retryCount);
        if (status === 429) {
          const retryAfter = error.response?.headers?.['retry-after'];
          if (retryAfter) {
            const parsed = Number(retryAfter);
            if (!Number.isNaN(parsed)) {
              delay = parsed * 1000;
            }
          }
        }

        await new Promise((resolve) => setTimeout(resolve, delay));
        return api.request(config);
      }
    }

    // Existing 401 logout behavior
    if (status === 401) {
      useAuthStore.getState().logout();
      window.location.href = '/login';
    }

    return Promise.reject(error);
  }
);

export default api;
