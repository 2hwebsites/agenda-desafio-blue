<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import Card from 'primevue/card'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Password from 'primevue/password'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const username = ref('')
const password = ref('')
const loading = ref(false)
const errorMessage = ref<string | null>(null)

async function handleSubmit() {
  errorMessage.value = null
  loading.value = true
  try {
    await authStore.login(username.value, password.value)
    router.push('/contacts')
  } catch {
    errorMessage.value = 'Usuário ou senha inválidos.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-wrapper">
    <Card class="login-card">
      <template #title>Agenda de Contatos</template>
      <template #content>
        <form @submit.prevent="handleSubmit" class="login-form">
          <Message v-if="errorMessage" severity="error">{{ errorMessage }}</Message>

          <div class="field">
            <label for="username">Usuário</label>
            <InputText
              id="username"
              v-model="username"
              placeholder="Usuário"
              autocomplete="username"
              fluid
            />
          </div>

          <div class="field">
            <label for="password">Senha</label>
            <Password
              id="password"
              v-model="password"
              placeholder="Senha"
              :feedback="false"
              autocomplete="current-password"
              fluid
            />
          </div>

          <Button
            type="submit"
            label="Entrar"
            icon="pi pi-sign-in"
            :loading="loading"
            fluid
          />
        </form>
      </template>
    </Card>
  </div>
</template>

<style scoped>
.login-wrapper {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--p-surface-ground, #f8f9fa);
}

.login-card {
  width: 100%;
  max-width: 380px;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.field label {
  font-weight: 500;
  font-size: 0.875rem;
}
</style>
