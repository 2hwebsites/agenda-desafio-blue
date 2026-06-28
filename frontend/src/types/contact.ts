export interface Contact {
  id: string
  name: string
  email: string
  phone: string | null
  createdAt: string
}

export interface ContactPayload {
  name: string
  email: string
  phone: string | null
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}
