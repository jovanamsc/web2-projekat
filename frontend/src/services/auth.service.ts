import api from './api';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../models/user.models';

function mapUser(raw: { firstName?: string; lastName?: string; [key: string]: unknown }) {
  const { firstName = '', lastName = '', ...rest } = raw;
  return { ...rest, name: `${firstName} ${lastName}`.trim() };
}

const authService = {
  async login(data: LoginRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/api/auth/login', data);
    const raw = response.data as unknown as { token: string; user: { firstName?: string; lastName?: string; [key: string]: unknown } };
    return { token: raw.token, user: mapUser(raw.user) as AuthResponse['user'] };
  },

  async register(data: RegisterRequest): Promise<AuthResponse> {
    const parts = data.name.trim().split(/\s+/);
    const firstName = parts[0] ?? '';
    const lastName = parts.slice(1).join(' ') || firstName;
    const response = await api.post('/api/auth/register', { firstName, lastName, email: data.email, password: data.password });
    const raw = response.data as { token: string; user: { firstName?: string; lastName?: string; [key: string]: unknown } };
    return { token: raw.token, user: mapUser(raw.user) as AuthResponse['user'] };
  },
};

export default authService;
