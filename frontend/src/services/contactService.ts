import http from './http'
import type { Contact, PagedResult } from '@/types/contact'

export async function getContacts(params: {
  search?: string
  page: number
  pageSize: number
}): Promise<PagedResult<Contact>> {
  const { data } = await http.get<PagedResult<Contact>>('/api/contacts', { params })
  return data
}
