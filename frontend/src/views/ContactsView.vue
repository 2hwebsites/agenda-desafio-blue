<script setup lang="ts">
import { onMounted, watch } from 'vue'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { useContacts } from '@/composables/useContacts'

const authStore = useAuthStore()
const { contacts, loading, totalRecords, search, page, pageSize, errorMessage, fetchContacts } =
  useContacts()

onMounted(fetchContacts)

let debounceTimer: ReturnType<typeof setTimeout> | null = null
watch(search, () => {
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    page.value = 1
    fetchContacts()
  }, 400)
})

function onPage(event: { page: number; rows: number }) {
  page.value = event.page + 1
  pageSize.value = event.rows
  fetchContacts()
}

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString('pt-BR')
}
</script>

<template>
  <div class="contacts-container">
    <header class="contacts-header">
      <h1>Contatos</h1>
      <Button label="Sair" icon="pi pi-sign-out" severity="secondary" @click="authStore.logout()" />
    </header>

    <div class="contacts-toolbar">
      <InputText v-model="search" placeholder="Buscar por nome ou e-mail..." />
    </div>

    <Message v-if="errorMessage" severity="error">{{ errorMessage }}</Message>

    <DataTable
      :value="contacts"
      :loading="loading"
      lazy
      paginator
      :rows="pageSize"
      :totalRecords="totalRecords"
      :first="(page - 1) * pageSize"
      @page="onPage"
    >
      <template #empty>Nenhum contato encontrado.</template>

      <Column field="name" header="Nome" />
      <Column field="email" header="E-mail" />
      <Column field="phone" header="Telefone" />
      <Column header="Criado em">
        <template #body="{ data }">
          {{ formatDate(data.createdAt) }}
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<style scoped>
.contacts-container {
  padding: 2rem;
}

.contacts-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 1.5rem;
}

.contacts-header h1 {
  margin: 0;
}

.contacts-toolbar {
  margin-bottom: 1rem;
}
</style>
