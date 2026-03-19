import axios from 'axios';
import type { HealthCheck } from '../types';

export const healthApi = {
  check: () => axios.get<HealthCheck>('/health'),
};
