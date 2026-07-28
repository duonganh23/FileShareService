<script setup>
import { ref, computed } from 'vue'
import DragDropZone from '@/components/DragDropZone.vue'
import ProgressBar from '@/components/ProgressBar.vue'
import { filesApi } from '@/api/files'
import { addToHistory } from '@/utils/history'

const selectedFile = ref(null)
const expiry = ref('1d') // 1h | 1d | 1w | never
const maxDownloads = ref(0)
const passwordEnabled = ref(false)
const password = ref('')

const uploading = ref(false)
const progress = ref(0)
const error = ref('')
const result = ref(null) // upload response metadata
const copied = ref(false)

const shareUrl = computed(() =>
  result.value ? `${window.location.origin}/f/${result.value.code}` : '',
)

function computeExpiresAt() {
  const now = Date.now()
  const map = { '1h': 3600e3, '1d': 86400e3, '1w': 604800e3 }
  if (expiry.value === 'never') return null
  return new Date(now + map[expiry.value]).toISOString()
}

function onFileSelected(file) {
  selectedFile.value = file
  result.value = null
  error.value = ''
}

async function upload() {
  if (!selectedFile.value) return
  uploading.value = true
  progress.value = 0
  error.value = ''
  try {
    const data = await filesApi.upload(
      selectedFile.value,
      { maxDownloads: Number(maxDownloads.value) || 0, expiresAt: computeExpiresAt() },
      (evt) => {
        if (evt.total) progress.value = (evt.loaded / evt.total) * 100
      },
    )
    result.value = data
    addToHistory({
      code: data.code,
      originalFileName: data.originalFileName,
      mimeType: data.mimeType,
      sizeBytes: data.sizeBytes,
      maxDownloads: data.maxDownloads,
      expiresAt: data.expiresAt,
      createdAt: data.createdAt,
    })
    // Auto-copy the share link to the clipboard per the brief.
    try {
      await navigator.clipboard.writeText(`${window.location.origin}/f/${data.code}`)
      copied.value = true
    } catch {
      /* clipboard may be blocked without user gesture; link is still shown */
    }
  } catch (e) {
    error.value = e?.response?.data || e.message || 'Upload failed.'
  } finally {
    uploading.value = false
  }
}

async function copyLink() {
  await navigator.clipboard.writeText(shareUrl.value)
  copied.value = true
}

function reset() {
  selectedFile.value = null
  result.value = null
  progress.value = 0
  error.value = ''
  copied.value = false
}

function humanSize(bytes) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}
</script>

<template>
  <div class="layout">
    <section class="main">
      <h1>Upload Files</h1>
      <p class="muted">
        Share a file, get a short link, and set expiry or download limits.
      </p>

      <!-- Success panel -->
      <div v-if="result" class="card success">
        <h2>✅ Transfer Complete</h2>
        <p class="muted">
          {{ result.originalFileName }} · {{ humanSize(result.sizeBytes) }}
        </p>
        <label class="label">Shareable link</label>
        <div class="link-row">
          <input class="input" :value="shareUrl" readonly />
          <button class="btn" @click="copyLink">{{ copied ? 'Copied!' : 'Copy Link' }}</button>
        </div>
        <button class="btn btn-secondary" style="margin-top: 16px" @click="reset">
          Share another
        </button>
      </div>

      <!-- Upload flow -->
      <template v-else>
        <DragDropZone v-if="!selectedFile" @file-selected="onFileSelected" />

        <div v-else class="card selected">
          <div class="file-line">
            <span class="fname">📄 {{ selectedFile.name }}</span>
            <span class="muted">{{ humanSize(selectedFile.size) }}</span>
          </div>
          <ProgressBar
            v-if="uploading"
            :percent="progress"
            :label="`Uploading ${selectedFile.name}`"
          />
          <div class="actions">
            <button class="btn" :disabled="uploading" @click="upload">
              {{ uploading ? 'Uploading…' : 'Generate Secure Link' }}
            </button>
            <button class="btn btn-secondary" :disabled="uploading" @click="reset">Cancel</button>
          </div>
        </div>

        <p v-if="error" class="error">{{ error }}</p>
      </template>
    </section>

    <!-- Security configuration -->
    <aside class="card config">
      <h3>Security Configuration</h3>

      <label class="label">Link Expiration</label>
      <select v-model="expiry" class="select">
        <option value="1h">1 Hour</option>
        <option value="1d">24 Hours</option>
        <option value="1w">1 Week</option>
        <option value="never">Never</option>
      </select>

      <label class="label" style="margin-top: 16px">Download Limit</label>
      <input v-model.number="maxDownloads" type="number" min="0" class="input" />
      <p class="hint muted">0 = unlimited. The link self-destructs after this many downloads.</p>

      <label class="label" style="margin-top: 16px">
        <input type="checkbox" v-model="passwordEnabled" /> Password Protection
      </label>
      <input
        v-if="passwordEnabled"
        v-model="password"
        type="password"
        class="input"
        placeholder="Enter passphrase"
      />
      <p v-if="passwordEnabled" class="hint muted">
        Enter a Password
      </p>
    </aside>
  </div>
</template>

<style scoped>
.layout {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 24px;
  align-items: start;
}
@media (max-width: 760px) {
  .layout {
    grid-template-columns: 1fr;
  }
}
h1 {
  margin: 0 0 4px;
}
.success h2 {
  margin-top: 0;
}
.link-row {
  display: flex;
  gap: 8px;
}
.selected .file-line {
  display: flex;
  justify-content: space-between;
  margin-bottom: 16px;
}
.fname {
  font-weight: 600;
}
.actions {
  display: flex;
  gap: 8px;
  margin-top: 16px;
}
.config h3 {
  margin-top: 0;
}
.hint {
  font-size: 12px;
  margin: 6px 0 0;
}
.error {
  color: var(--danger);
  margin-top: 12px;
}
</style>
