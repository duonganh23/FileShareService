<script setup>
import { ref } from 'vue'

const emit = defineEmits(['submit'])
defineProps({
  error: { type: String, default: '' },
})

const value = ref('')
</script>

<template>
  <div class="overlay">
    <div class="card modal">
      <div class="lock">🔒</div>
      <h2>This file is encrypted</h2>
      <p class="muted">Accessing this file requires a passphrase provided by the sender.</p>
      <input
        v-model="value"
        type="password"
        class="input"
        placeholder="Enter security passphrase"
        @keyup.enter="emit('submit', value)"
      />
      <p v-if="error" class="err">{{ error }}</p>
      <button class="btn" style="margin-top: 12px; width: 100%" @click="emit('submit', value)">
        Unlock Payload
      </button>
    </div>
  </div>
</template>

<style scoped>
.overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
}
.modal {
  max-width: 420px;
  width: 100%;
  text-align: center;
}
.lock {
  font-size: 36px;
}
.modal h2 {
  margin: 8px 0 4px;
}
.err {
  color: var(--danger);
  font-size: 14px;
}
</style>
