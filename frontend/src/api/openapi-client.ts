/**
 * Typed API client using auto-generated OpenAPI types.
 *
 * Usage:
 * 1. Start the API: `dotnet run --project src/api/RVR.Framework.Api`
 * 2. Generate types: `npm run api:generate`
 * 3. Import and use: `import { apiClient } from './api/openapi-client'`
 *
 * For now, the manual Axios-based clients in auth.ts, users.ts, etc.
 * are used. This typed client is available for migration when needed.
 */
import axios from 'axios';
import { useAuthStore } from '../store/authStore';

const openApiClient = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
});

openApiClient.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export default openApiClient;
