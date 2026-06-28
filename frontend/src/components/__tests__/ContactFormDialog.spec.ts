import { describe, it, expect, afterEach } from 'vitest'
import { mount, type VueWrapper } from '@vue/test-utils'
import PrimeVue from 'primevue/config'
import ContactFormDialog from '../ContactFormDialog.vue'
import type { Contact } from '@/types/contact'

const mockContact: Contact = {
  id: '1',
  name: 'João Silva',
  email: 'joao@example.com',
  phone: '11999999999',
  createdAt: '2026-01-01T00:00:00Z',
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
let wrapper: VueWrapper<any>

function createWrapper(props: Record<string, unknown> = {}) {
  return mount(ContactFormDialog, {
    props: {
      visible: true,
      contact: null,
      loading: false,
      emailError: null,
      ...props,
    },
    global: {
      plugins: [PrimeVue],
      stubs: {
        // Stub Dialog so its slot renders directly without Teleport
        Dialog: { template: '<div><slot /></div>' },
      },
    },
    attachTo: document.body,
  })
}

afterEach(() => {
  wrapper?.unmount()
})

describe('ContactFormDialog', () => {
  describe('creation mode (contact = null)', () => {
    it('does not emit submit and shows name error when name is empty', async () => {
      wrapper = createWrapper()

      await wrapper.find('input#cf-email').setValue('valid@example.com')
      await wrapper.find('form').trigger('submit')

      expect(wrapper.emitted('submit')).toBeUndefined()
      expect(wrapper.text()).toContain('Nome deve ter entre 2 e 150 caracteres.')
    })

    it('does not emit submit and shows email error when email is invalid', async () => {
      wrapper = createWrapper()

      await wrapper.find('input#cf-name').setValue('Valid Name')
      await wrapper.find('input#cf-email').setValue('not-an-email')
      await wrapper.find('form').trigger('submit')

      expect(wrapper.emitted('submit')).toBeUndefined()
      expect(wrapper.text()).toContain('Informe um e-mail válido.')
    })

    it('emits submit with correct payload when form is valid with phone', async () => {
      wrapper = createWrapper()

      await wrapper.find('input#cf-name').setValue('Maria Souza')
      await wrapper.find('input#cf-email').setValue('maria@example.com')
      await wrapper.find('input#cf-phone').setValue('11988887777')
      await wrapper.find('form').trigger('submit')

      expect(wrapper.emitted('submit')).toStrictEqual([
        [{ name: 'Maria Souza', email: 'maria@example.com', phone: '11988887777' }],
      ])
    })

    it('emits submit with phone=null when phone field is left empty', async () => {
      wrapper = createWrapper()

      await wrapper.find('input#cf-name').setValue('Maria Souza')
      await wrapper.find('input#cf-email').setValue('maria@example.com')
      await wrapper.find('form').trigger('submit')

      expect(wrapper.emitted('submit')).toMatchObject([[{ phone: null }]])
    })
  })

  describe('edit mode (contact provided)', () => {
    it('pre-fills fields when visible and contact props are set', async () => {
      wrapper = createWrapper({ visible: false, contact: null })
      await wrapper.setProps({ visible: true, contact: mockContact })

      expect((wrapper.find('input#cf-name').element as HTMLInputElement).value).toBe(
        mockContact.name,
      )
      expect((wrapper.find('input#cf-email').element as HTMLInputElement).value).toBe(
        mockContact.email,
      )
      expect((wrapper.find('input#cf-phone').element as HTMLInputElement).value).toBe(
        mockContact.phone,
      )
    })
  })

  describe('emailError prop', () => {
    it('shows server error in email field when emailError prop is set on mount', () => {
      // immediate: true on the watcher means the error renders as soon as the prop is present
      wrapper = createWrapper({ emailError: 'Já existe um contato com este e-mail.' })

      expect(wrapper.text()).toContain('Já existe um contato com este e-mail.')
    })

    it('shows server error when emailError prop changes after mount', async () => {
      wrapper = createWrapper({ emailError: null })

      expect(wrapper.text()).not.toContain('Já existe um contato')

      await wrapper.setProps({ emailError: 'Já existe um contato com este e-mail.' })

      expect(wrapper.text()).toContain('Já existe um contato com este e-mail.')
    })

    it('clears the email field error when the user types in the email input', async () => {
      // Mount with error already present; immediate watcher displays it right away
      wrapper = createWrapper({ emailError: 'Já existe um contato com este e-mail.' })

      expect(wrapper.text()).toContain('Já existe um contato com este e-mail.')

      await wrapper.find('input#cf-email').trigger('input')

      expect(wrapper.text()).not.toContain('Já existe um contato com este e-mail.')
    })
  })
})
