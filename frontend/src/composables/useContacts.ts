import { ref } from 'vue'
import type { Contact, ContactPayload } from '@/types/contact'
import {
  getContacts,
  createContact as apiCreate,
  updateContact as apiUpdate,
  deleteContact as apiDelete,
} from '@/services/contactService'

export function useContacts() {
  const contacts = ref<Contact[]>([])
  const loading = ref(false)
  const totalRecords = ref(0)
  const search = ref('')
  const page = ref(1)
  const pageSize = ref(10)
  const errorMessage = ref<string | null>(null)

  async function fetchContacts() {
    errorMessage.value = null
    loading.value = true
    try {
      const result = await getContacts({
        search: search.value || undefined,
        page: page.value,
        pageSize: pageSize.value,
      })
      contacts.value = result.items
      totalRecords.value = result.totalCount
    } catch {
      errorMessage.value = 'Erro ao carregar contatos.'
    } finally {
      loading.value = false
    }
  }

  async function createContact(payload: ContactPayload): Promise<void> {
    await apiCreate(payload)
    await fetchContacts()
  }

  async function updateContact(id: string, payload: ContactPayload): Promise<void> {
    await apiUpdate(id, payload)
    await fetchContacts()
  }

  async function removeContact(id: string): Promise<void> {
    await apiDelete(id)
    await fetchContacts()
  }

  return {
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
  }
}
