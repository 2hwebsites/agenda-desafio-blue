import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import { login as apiLogin } from '@/services/authService'
import { TOKEN_KEY } from '@/services/http'
import router from '@/router'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))

  const isAuthenticated = computed(() => token.value !== null)

  async function login(username: string, password: string): Promise<void> {
    const response = await apiLogin(username, password)
    token.value = response.token
    localStorage.setItem(TOKEN_KEY, response.token)
  }

  function logout(): void {
    token.value = null
    localStorage.removeItem(TOKEN_KEY)
    router.push('/login')
  }

  return { token, isAuthenticated, login, logout }
})
