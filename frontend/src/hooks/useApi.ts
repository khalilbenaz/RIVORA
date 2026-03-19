import { useState, useEffect, useCallback } from 'react';
import type { AxiosError, AxiosResponse } from 'axios';
import type { ApiError } from '../types/errors';

interface UseApiResult<T> {
  data: T | null;
  loading: boolean;
  error: ApiError | null;
  refetch: () => Promise<void>;
}

function extractApiError(err: unknown): ApiError {
  // Check if it's an AxiosError with a response body
  const axiosErr = err as AxiosError<{ message?: string; errors?: Record<string, string[]> }>;
  if (axiosErr?.response?.data) {
    const { data, status } = axiosErr.response;
    return {
      message: data.message ?? axiosErr.message ?? 'An error occurred',
      errors: data.errors,
      statusCode: status,
    };
  }

  // Fallback for network errors or non-Axios errors
  if (axiosErr?.response?.status) {
    return {
      message: axiosErr.message ?? 'An error occurred',
      statusCode: axiosErr.response.status,
    };
  }

  return {
    message: err instanceof Error ? err.message : 'An error occurred',
  };
}

export function useApi<T>(
  fetcher: () => Promise<AxiosResponse<T>>,
  deps: unknown[] = []
): UseApiResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<ApiError | null>(null);

  const refetch = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await fetcher();
      setData(res.data);
    } catch (err: unknown) {
      setError(extractApiError(err));
    } finally {
      setLoading(false);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  useEffect(() => {
    void refetch();
  }, [refetch]);

  return { data, loading, error, refetch };
}
