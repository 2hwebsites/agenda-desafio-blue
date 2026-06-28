<script setup lang="ts">
import { onMounted, ref, watch } from 'vue'
import axios from 'axios'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'
import Button from 'primevue/button'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import { useAuthStore } from '@/stores/auth'
import { useContacts } from '@/composables/useContacts'
import ContactFormDialog from '@/components/ContactFormDialog.vue'
import type { Contact, ContactPayload } from '@/types/contact'

const authStore = useAuthStore()
const toast = useToast()
const confirm = useConfirm()

const {
  contacts,
  loading,
  totalRecords,
  search,
  page,
  pageSize,
  errorMessage,
  fetchContacts,
  createContact,
  updateContact,
  removeContact,
} = useContacts()

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

const dialogVisible = ref(false)
const editingContact = ref<Contact | null>(null)
const saveLoading = ref(false)
const saveEmailError = ref<string | null>(null)

watch(dialogVisible, (visible) => {
  if (!visible) saveEmailError.value = null
})

function openCreate() {
  editingContact.value = null
  dialogVisible.value = true
}

function openEdit(contact: Contact) {
  editingContact.value = contact
  dialogVisible.value = true
}

async function handleSubmit(payload: ContactPayload) {
  saveLoading.value = true
  saveEmailError.value = null
  try {
    if (editingContact.value) {
      await updateContact(editingContact.value.id, payload)
      toast.add({ severity: 'success', summary: 'Contato atualizado', life: 3000 })
    } else {
      await createContact(payload)
      toast.add({ severity: 'success', summary: 'Contato criado', life: 3000 })
    }
    dialogVisible.value = false
  } catch (err) {
    if (axios.isAxiosError(err)) {
      const status = err.response?.status
      if (status === 409) {
        saveEmailError.value = 'Já existe um contato com este e-mail.'
      } else if (status === 400) {
        toast.add({
          severity: 'error',
          summary: 'Dados inválidos',
          detail: 'Verifique os campos e tente novamente.',
          life: 4000,
        })
      } else {
        toast.add({ severity: 'error', summary: 'Erro ao salvar', detail: 'Tente novamente.', life: 4000 })
      }
    } else {
      toast.add({ severity: 'error', summary: 'Erro ao salvar', detail: 'Tente novamente.', life: 4000 })
    }
  } finally {
    saveLoading.value = false
  }
}

function handleDelete(contact: Contact) {
  confirm.require({
    message: 'Deseja excluir este contato?',
    header: 'Confirmar exclusão',
    icon: 'pi pi-exclamation-triangle',
    acceptLabel: 'Excluir',
    rejectLabel: 'Cancelar',
    accept: async () => {
      try {
        await removeContact(contact.id)
        toast.add({ severity: 'success', summary: 'Contato excluído', life: 3000 })
      } catch {
        toast.add({
          severity: 'error',
          summary: 'Erro ao excluir',
          detail: 'Tente novamente.',
          life: 4000,
        })
      }
    },
  })
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
      <Button label="Novo contato" icon="pi pi-plus" @click="openCreate" />
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
      <Column header="Ações" style="width: 8rem">
        <template #body="{ data }">
          <Button icon="pi pi-pencil" text rounded severity="secondary" @click="openEdit(data)" />
          <Button icon="pi pi-trash" text rounded severity="danger" @click="handleDelete(data)" />
        </template>
      </Column>
    </DataTable>

    <ContactFormDialog
      v-model:visible="dialogVisible"
      :contact="editingContact"
      :loading="saveLoading"
      :emailError="saveEmailError"
      @submit="handleSubmit"
    />
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
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
}
</style>
