export interface User {
  id: string;
  userName: string;
  email: string;
  firstName: string | null;
  lastName: string | null;
  isActive: boolean;
  emailConfirmed: boolean;
  twoFactorEnabled: boolean;
  createdAt: string;
  tenantId?: string | null;
}

export interface Product {
  id: string;
  name: string;
  sku: string | null;
  description: string | null;
  category: string | null;
  price: number;
  stock: number;
  isActive: boolean;
  createdAt: string;
}

export interface AuditLog {
  id: string;
  userId: string | null;
  userName: string | null;
  httpMethod: string | null;
  url: string | null;
  httpStatusCode: number | null;
  ipAddress: string | null;
  executionDate: string;
  executionTime: number;
  exceptionMessage: string | null;
}

export interface Tenant {
  id: string;
  name: string;
  identifier: string;
  connectionString: string | null;
  isActive: boolean;
  createdAt: string;
}

export interface HealthCheck {
  status: string;
  totalDuration: string;
  entries: Record<string, HealthEntry>;
}

export interface HealthEntry {
  status: string;
  duration: string;
  description: string | null;
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiration: string;
  user: User;
}

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface InitStatus {
  needsInitialization: boolean;
  userCount: number;
  message: string;
}
