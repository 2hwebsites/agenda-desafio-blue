<script setup lang="ts">
import Button from 'primevue/button'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
</script>

<template>
  <main style="padding: 2rem">
    <div style="display: flex; align-items: center; justify-content: space-between">
      <h1>Contatos</h1>
      <Button label="Sair" icon="pi pi-sign-out" severity="secondary" @click="authStore.logout()" />
    </div>
  </main>
</template>
