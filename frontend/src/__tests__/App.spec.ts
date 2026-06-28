import { describe, it, expect } from 'vitest'
import { mount } from '@vue/test-utils'
import App from '../App.vue'

describe('App', () => {
  it('mounts without error', () => {
    const wrapper = mount(App, {
      global: {
        stubs: {
          RouterView: { template: '<div />' },
          Toast: { template: '<div />' },
          ConfirmDialog: { template: '<div />' },
        },
      },
    })
    expect(wrapper.exists()).toBe(true)
  })
})
