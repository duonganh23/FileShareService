<script setup>
import { ref } from 'vue'
import { getHistory, removeFromHistory } from '@/utils/history'
import { filesApi } from '@/api/files'

const items = ref(getHistory())
const copiedCode = ref('')

function shareUrl(code) {
  return `${window.location.origin}/f/${code}`
}

async function copy(code) {
  await navigator.clipboard.writeText(shareUrl(code))
  copiedCode.value = code
  setTimeout(() => (copiedCode.value = ''), 1500)
}

async function remove(code) {
  if (!confirm('Delete this file permanently?')) return
  try {
    await filesApi.remove(code)
  } catch {
    /* already gone on the server — still drop it from local history */
  }
  items.value = removeFromHistory(code)
}

function humanSize(bytes) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}
</script>

<template>
  <div>
    <h1>Upload History</h1>
    <p class="muted">Files you've shared from this browser.</p>

    <div v-if="items.length === 0" class="card center">
      <p class="muted">No uploads yet.</p>
      <RouterLink to="/" class="btn" style="text-decoration: none">Upload a file</RouterLink>
    </div>

    <div v-else class="card list">
      <div v-for="item in items" :key="item.code" class="row">
        <div class="info">
          <RouterLink :to="`/f/${item.code}`" class="name">{{ item.originalFileName }}</RouterLink>
          <span class="muted sub">
            {{ humanSize(item.sizeBytes) }} · {{ new Date(item.createdAt).toLocaleString() }}
          </span>
        </div>
        <div class="ops">
          <button
            class="link-copy"
            :class="{ copied: copiedCode === item.code }"
            @click="copy(item.code)"
            :title="`Copy link to ${item.originalFileName}`"
          >
            <code>/f/{{ item.code }}</code>
            <span class="copy-icon">{{ copiedCode === item.code ? '✓' : '📋' }}</span>
          </button>
          <button class="btn btn-danger sm" @click="remove(item.code)">Delete</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.center {
  text-align: center;
}
.center .btn {
  margin-top: 10px;
}
.row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  padding: 14px 0;
  border-bottom: 1px solid var(--border);
  flex-wrap: wrap;
}
.row:last-child {
  border-bottom: none;
}
.name {
  font-weight: 600;
  text-decoration: none;
  display: block;
}
.sub {
  font-size: 13px;
}
.ops {
  display: flex;
  align-items: center;
  gap: 8px;
}
.link-copy {
  background: var(--bg);
  border: 1px solid var(--border);
  padding: 6px 10px;
  border-radius: 8px;
  font-size: 13px;
  cursor: pointer;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: all 0.2s ease;
  font-family: monospace;
}
.link-copy:hover {
  background: var(--primary);
  color: var(--primary-contrast);
  border-color: var(--primary);
}
.link-copy.copied {
  background: var(--success);
  color: white;
  border-color: var(--success);
}
.copy-icon {
  font-size: 14px;
  flex-shrink: 0;
}
code {
  font-size: 12px;
}
.btn.sm {
  padding: 6px 12px;
  font-size: 13px;
}
</style>
