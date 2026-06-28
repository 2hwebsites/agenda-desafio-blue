<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import Button from 'primevue/button'
import Dialog from 'primevue/dialog'
import InputText from 'primevue/inputtext'
import type { Contact, ContactPayload } from '@/types/contact'

const props = withDefaults(
  defineProps<{
    visible: boolean
    contact: Contact | null
    loading: boolean
    emailError: string | null
  }>(),
  { emailError: null },
)

const emit = defineEmits<{
  'update:visible': [value: boolean]
  submit: [payload: ContactPayload]
}>()

const dialogVisible = computed({
  get: () => props.visible,
  set: (value) => emit('update:visible', value),
})

const header = computed(() => (props.contact ? 'Editar contato' : 'Novo contato'))

const name = ref('')
const email = ref('')
const phone = ref('')

const nameError = ref<string | null>(null)
const emailFieldError = ref<string | null>(null)

watch(
  () => props.emailError,
  (err) => {
    emailFieldError.value = err
  },
)

watch([() => props.visible, () => props.contact], ([visible]) => {
  if (visible) {
    name.value = props.contact?.name ?? ''
    email.value = props.contact?.email ?? ''
    phone.value = props.contact?.phone ?? ''
    nameError.value = null
    emailFieldError.value = null
  }
})

function validateEmailFormat(value: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)
}

function validate(): boolean {
  nameError.value = null
  emailFieldError.value = null
  let valid = true

  const trimmedName = name.value.trim()
  if (!trimmedName || trimmedName.length < 2 || trimmedName.length > 150) {
    nameError.value = 'Nome deve ter entre 2 e 150 caracteres.'
    valid = false
  }

  const trimmedEmail = email.value.trim()
  if (!trimmedEmail) {
    emailFieldError.value = 'E-mail é obrigatório.'
    valid = false
  } else if (!validateEmailFormat(trimmedEmail)) {
    emailFieldError.value = 'Informe um e-mail válido.'
    valid = false
  }

  return valid
}

function handleSubmit() {
  if (!validate()) return
  emit('submit', {
    name: name.value.trim(),
    email: email.value.trim(),
    phone: phone.value.trim() || null,
  })
}

function close() {
  emit('update:visible', false)
}
</script>

<template>
  <Dialog v-model:visible="dialogVisible" :header="header" modal :style="{ width: '32rem' }">
    <form @submit.prevent="handleSubmit" class="contact-form">
      <div class="field">
        <label for="cf-name">Nome *</label>
        <InputText id="cf-name" v-model="name" :invalid="!!nameError" fluid />
        <small v-if="nameError" class="field-error">{{ nameError }}</small>
      </div>

      <div class="field">
        <label for="cf-email">E-mail *</label>
        <InputText
          id="cf-email"
          v-model="email"
          :invalid="!!emailFieldError"
          fluid
          @input="emailFieldError = null"
        />
        <small v-if="emailFieldError" class="field-error">{{ emailFieldError }}</small>
      </div>

      <div class="field">
        <label for="cf-phone">Telefone</label>
        <InputText id="cf-phone" v-model="phone" fluid />
      </div>

      <div class="dialog-footer">
        <Button
          label="Cancelar"
          severity="secondary"
          type="button"
          :disabled="loading"
          @click="close"
        />
        <Button label="Salvar" type="submit" :loading="loading" />
      </div>
    </form>
  </Dialog>
</template>

<style scoped>
.contact-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  padding-top: 0.5rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.field label {
  font-size: 0.875rem;
  font-weight: 500;
}

.field-error {
  color: var(--p-red-500, #ef4444);
  font-size: 0.75rem;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 0.5rem;
  margin-top: 0.5rem;
}
</style>
