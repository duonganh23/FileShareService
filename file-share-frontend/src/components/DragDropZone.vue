<script setup>
import { ref } from 'vue'

const emit = defineEmits(['file-selected'])
const props = defineProps({
  maxSizeBytes: { type: Number, default: 10 * 1024 * 1024 }, // 10 MB
})

const isDragging = ref(false)
const error = ref('')
const inputRef = ref(null)

function validateAndEmit(file) {
  error.value = ''
  if (!file) return
  if (file.size > props.maxSizeBytes) {
    const mb = (props.maxSizeBytes / (1024 * 1024)).toFixed(0)
    error.value = `File exceeds the ${mb} MB limit.`
    return
  }
  emit('file-selected', file)
}

function onDrop(e) {
  isDragging.value = false
  const file = e.dataTransfer?.files?.[0]
  validateAndEmit(file)
}

function onChange(e) {
  validateAndEmit(e.target.files?.[0])
}

function openPicker() {
  inputRef.value?.click()
}
</script>

<template>
  <div>
    <div
      class="dropzone"
      :class="{ dragging: isDragging }"
      @dragover.prevent="isDragging = true"
      @dragleave.prevent="isDragging = false"
      @drop.prevent="onDrop"
      @click="openPicker"
    >
      <div class="icon">☁️</div>
      <h3>Drag &amp; drop a file to upload</h3>
      <p class="muted">Any file type, up to 10 MB.</p>
      <button type="button" class="btn" @click.stop="openPicker">+ Browse Files</button>
      <input ref="inputRef" type="file" hidden @change="onChange" />
    </div>
    <p v-if="error" class="error">{{ error }}</p>
  </div>
</template>

<style scoped>
.dropzone {
  border: 2px dashed var(--border);
  border-radius: var(--radius);
  padding: 48px 24px;
  text-align: center;
  cursor: pointer;
  transition: border-color 0.15s ease, background 0.15s ease;
}
.dropzone.dragging {
  border-color: var(--accent);
  background: rgba(37, 99, 235, 0.04);
}
.icon {
  font-size: 40px;
}
.dropzone h3 {
  margin: 12px 0 4px;
}
.dropzone .btn {
  margin-top: 16px;
}
.error {
  color: var(--danger);
  font-size: 14px;
  margin-top: 10px;
}
</style>
