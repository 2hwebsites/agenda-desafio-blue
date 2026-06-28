import { describe, it, expect, vi, beforeEach } from 'vitest'

vi.mock('@/services/contactService', () => ({
  getContacts: vi.fn(),
  createContact: vi.fn(),
  updateContact: vi.fn(),
  deleteContact: vi.fn(),
}))

import { useContacts } from '@/composables/useContacts'
import {
  getContacts,
  createContact as svcCreate,
  updateContact as svcUpdate,
  deleteContact,
} from '@/services/contactService'

const mockContact = {
  id: '1',
  name: 'Alice',
  email: 'alice@example.com',
  phone: null,
  createdAt: '2026-01-01T00:00:00Z',
}

const mockPagedResult = {
  items: [mockContact],
  page: 1,
  pageSize: 10,
  totalCount: 1,
  totalPages: 1,
}

describe('useContacts', () => {
  beforeEach(() => {
    vi.resetAllMocks()
  })

  describe('fetchContacts', () => {
    it('populates contacts and totalRecords from PagedResult on success', async () => {
      vi.mocked(getContacts).mockResolvedValueOnce(mockPagedResult)

      const { contacts, totalRecords, fetchContacts } = useContacts()
      await fetchContacts()

      expect(contacts.value).toEqual(mockPagedResult.items)
      expect(totalRecords.value).toBe(1)
    })

    it('sets loading true during fetch and false when done', async () => {
      let resolve!: (v: typeof mockPagedResult) => void
      const deferred = new Promise<typeof mockPagedResult>((r) => {
        resolve = r
      })
      vi.mocked(getContacts).mockReturnValueOnce(deferred)

      const { loading, fetchContacts } = useContacts()

      const promise = fetchContacts()
      expect(loading.value).toBe(true)

      resolve(mockPagedResult)
      await promise

      expect(loading.value).toBe(false)
    })

    it('sets errorMessage on service error and does not propagate', async () => {
      vi.mocked(getContacts).mockRejectedValueOnce(new Error('Network error'))

      const { errorMessage, fetchContacts } = useContacts()

      await expect(fetchContacts()).resolves.toBeUndefined()
      expect(errorMessage.value).toBe('Erro ao carregar contatos.')
    })

    it('clears errorMessage before each fetch attempt', async () => {
      vi.mocked(getContacts).mockRejectedValueOnce(new Error('first'))
      vi.mocked(getContacts).mockResolvedValueOnce(mockPagedResult)

      const { errorMessage, fetchContacts } = useContacts()

      await fetchContacts()
      expect(errorMessage.value).toBe('Erro ao carregar contatos.')

      await fetchContacts()
      expect(errorMessage.value).toBeNull()
    })
  })

  describe('createContact', () => {
    it('calls fetchContacts again after successful creation', async () => {
      vi.mocked(svcCreate).mockResolvedValueOnce(mockContact)
      vi.mocked(getContacts).mockResolvedValueOnce(mockPagedResult)

      const { createContact } = useContacts()
      await createContact({ name: 'Alice', email: 'alice@example.com', phone: null })

      expect(getContacts).toHaveBeenCalledOnce()
    })

    it('propagates error on failure', async () => {
      vi.mocked(svcCreate).mockRejectedValueOnce(new Error('Conflict'))

      const { createContact } = useContacts()

      await expect(
        createContact({ name: 'Alice', email: 'alice@example.com', phone: null }),
      ).rejects.toThrow('Conflict')
    })
  })

  describe('updateContact', () => {
    it('calls fetchContacts again after successful update', async () => {
      vi.mocked(svcUpdate).mockResolvedValueOnce({ ...mockContact, name: 'Alice Updated' })
      vi.mocked(getContacts).mockResolvedValueOnce(mockPagedResult)

      const { updateContact } = useContacts()
      await updateContact('1', { name: 'Alice Updated', email: 'alice@example.com', phone: null })

      expect(getContacts).toHaveBeenCalledOnce()
    })

    it('propagates error on failure', async () => {
      vi.mocked(svcUpdate).mockRejectedValueOnce(new Error('Not found'))

      const { updateContact } = useContacts()

      await expect(
        updateContact('1', { name: 'X', email: 'x@x.com', phone: null }),
      ).rejects.toThrow('Not found')
    })
  })

  describe('removeContact', () => {
    it('calls fetchContacts again after successful deletion', async () => {
      vi.mocked(deleteContact).mockResolvedValueOnce(undefined)
      vi.mocked(getContacts).mockResolvedValueOnce(mockPagedResult)

      const { removeContact } = useContacts()
      await removeContact('1')

      expect(getContacts).toHaveBeenCalledOnce()
    })

    it('propagates error on failure', async () => {
      vi.mocked(deleteContact).mockRejectedValueOnce(new Error('Server error'))

      const { removeContact } = useContacts()

      await expect(removeContact('1')).rejects.toThrow('Server error')
    })
  })
})
