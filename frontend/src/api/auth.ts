import api from './client';
import type { LoginRequest, LoginResponse, InitStatus } from '../types';

export const authApi = {
  login: (data: LoginRequest) =>
    api.post<LoginResponse>('/auth/login', data),

  getInitStatus: () =>
    api.get<InitStatus>('/init/status'),
};
