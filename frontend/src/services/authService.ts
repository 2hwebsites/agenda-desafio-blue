import http from './http'

export interface LoginResponse {
  token: string
  expiresAt: string
}

export async function login(username: string, password: string): Promise<LoginResponse> {
  const { data } = await http.post<LoginResponse>('/api/auth/login', { username, password })
  return data
}
