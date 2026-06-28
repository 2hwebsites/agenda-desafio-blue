import http from './http'
import type { Contact, ContactPayload, PagedResult } from '@/types/contact'

export async function getContacts(params: {
  search?: string
  page: number
  pageSize: number
}): Promise<PagedResult<Contact>> {
  const { data } = await http.get<PagedResult<Contact>>('/api/contacts', { params })
  return data
}

export async function createContact(payload: ContactPayload): Promise<Contact> {
  const { data } = await http.post<Contact>('/api/contacts', payload)
  return data
}

export async function updateContact(id: string, payload: ContactPayload): Promise<Contact> {
  const { data } = await http.put<Contact>(`/api/contacts/${id}`, payload)
  return data
}

export async function deleteContact(id: string): Promise<void> {
  await http.delete(`/api/contacts/${id}`)
}
