export type Role = 'user' | 'admin';

export interface User {
  userId: number;
  departmentId?: number | null;
  fullName: string;
  email: string;
  phone?: string | null;
  isActive: boolean;
  role: Role;
  createdAt?: string | null;
}

export interface Zone {
  zoneId: number;
  name: string;
  description?: string | null;
  accessPointCount?: number;
  activeRuleCount?: number;
}

export interface AccessEvent {
  eventId: number;
  eventTime: string;
  userId?: number | null;
  accessPointId: number;
  credentialId?: number | null;
  result: string;
  reason?: string | null;
}

export interface AccessDecision {
  granted: boolean;
  result: string;
  reason?: string | null;
  utc?: string | null;
}

export interface AuthResponse {
  token: string;
  role: Role;
  user: User;
}

export interface ImportResult {
  imported: number;
  tables: number;
}
