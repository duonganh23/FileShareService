<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { filesApi } from '@/api/files'
import { removeFromHistory } from '@/utils/history'
import PasswordModal from '@/components/PasswordModal.vue'

const route = useRoute()
const router = useRouter()
const code = route.params.code

const loading = ref(true)
const meta = ref(null)
const errorStatus = ref(null) // 404 | 410 | other
const errorMessage = ref('')
const needsPassword = ref(false) // Distinction: shown on HTTP 401

const isImage = computed(() => meta.value?.mimeType?.startsWith('image/'))
const downloadUrl = computed(() => filesApi.downloadUrl(code))

async function load() {
  loading.value = true
  errorStatus.value = null
  try {
    meta.value = await filesApi.getInfo(code)
  } catch (e) {
    const status = e?.response?.status
    if (status === 401) {
      needsPassword.value = true
    } else {
      errorStatus.value = status || 'error'
      errorMessage.value =
        e?.response?.data ||
        (status === 404
          ? 'File not found.'
          : status === 410
            ? 'This link has expired or reached its download limit.'
            : 'Unable to load this file.')
    }
  } finally {
    loading.value = false
  }
}

function onPasswordSubmit() {
  // Backend password gate not yet implemented — placeholder for the Distinction flow.
  errorMessage.value = 'Password verification is not available (backend support pending).'
}

async function remove() {
  if (!confirm('Delete this file permanently?')) return
  await filesApi.remove(code)
  removeFromHistory(code)
  router.push('/history')
}

function humanSize(bytes) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

onMounted(load)
</script>

<template>
  <div>
    <PasswordModal v-if="needsPassword" :error="errorMessage" @submit="onPasswordSubmit" />

    <p v-if="loading" class="muted">Loading…</p>

    <div v-else-if="errorStatus" class="card center">
      <div class="big">{{ errorStatus === 404 ? '🔍' : '⏳' }}</div>
      <h2>{{ errorStatus === 410 ? 'Link no longer available' : 'Not found' }}</h2>
      <p class="muted">{{ errorMessage }}</p>
      <RouterLink to="/" class="btn" style="margin-top: 12px; text-decoration: none">
        Go to upload
      </RouterLink>
    </div>

    <div v-else-if="meta" class="card">
      <h1 class="fname">{{ meta.originalFileName }}</h1>
      <p class="muted meta-line">
        {{ humanSize(meta.sizeBytes) }} · {{ meta.mimeType }}
        <span v-if="meta.maxDownloads > 0">
          · {{ meta.downloadCount }}/{{ meta.maxDownloads }} downloads
        </span>
        <span v-if="meta.expiresAt"> · expires {{ new Date(meta.expiresAt).toLocaleString() }}</span>
      </p>

      <div v-if="isImage" class="preview">
        <img :src="downloadUrl" :alt="meta.originalFileName" />
      </div>
      <div v-else class="file-icon">
        <div class="big">📦</div>
        <p class="muted">Preview not available for this file type.</p>
      </div>

      <div class="actions">
        <a :href="downloadUrl" class="btn" download>⬇ Download</a>
        <button class="btn btn-danger" @click="remove">Delete File</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.center {
  text-align: center;
}
.big {
  font-size: 44px;
}
.fname {
  margin: 0 0 4px;
  word-break: break-all;
}
.meta-line {
  margin-top: 0;
}
.preview {
  margin: 16px 0;
  text-align: center;
}
.preview img {
  max-width: 100%;
  max-height: 480px;
  border-radius: var(--radius);
  border: 1px solid var(--border);
}
.file-icon {
  text-align: center;
  padding: 32px 0;
}
.actions {
  display: flex;
  gap: 8px;
  margin-top: 8px;
}
.actions .btn {
  text-decoration: none;
}
</style>
