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
    const enteredPassword = ref(null)

    const isImage = computed(() => meta.value?.mimeType?.startsWith('image/'))
    const downloadUrl = computed(() => filesApi.downloadUrl(code, enteredPassword.value))
    const downloadsRemaining = computed(() => {
        if (!meta.value || meta.value.maxDownloads === 0) return null
        return meta.value.maxDownloads - meta.value.downloadCount
    })
    const isLimitWarning = computed(() => downloadsRemaining.value !== null && downloadsRemaining.value <= 1)
    const isExpiringSoon = computed(() => {
        if (!meta.value?.expiresAt) return false
        const now = Date.now()
        const expires = new Date(meta.value.expiresAt).getTime()
        const minutesLeft = (expires - now) / 60000
        return minutesLeft > 0 && minutesLeft < 60 // Within 1 hour
    })

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

    async function onPasswordSubmit(password) {
        try {
            meta.value = await filesApi.getInfo(code, password)
            needsPassword.value = false
            enteredPassword.value = password
        } catch (e) {
            errorMessage.value = e?.response?.status === 401
                ? 'Incorrect password.'
                : 'Unable to verify password.'
        }
    }

    async function remove() {
        if (!confirm('Delete this file permanently?')) return
        try {
            await filesApi.remove(code)
            removeFromHistory(code)
            router.push('/history')
        } catch (e) {
            alert('Failed to delete file. Please try again.')
        }
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
      <!-- Warning banner: limit or expiry approaching -->
      <div v-if="isLimitWarning" class="banner warning">
        ⚠️ {{ downloadsRemaining }} download{{ downloadsRemaining !== 1 ? 's' : '' }} left before link expires!
      </div>
      <div v-if="isExpiringSoon" class="banner warning">
        ⏰ This link expires soon. Download now before it expires.
      </div>

      <h1 class="fname">{{ meta.originalFileName }}</h1>
      <p class="muted meta-line">
        {{ humanSize(meta.sizeBytes) }} · {{ meta.mimeType }}
      </p>

      <!-- Downloads remaining badge -->
      <div v-if="meta.maxDownloads > 0" class="badge-row">
        <span class="badge" :class="{ 'badge-warning': isLimitWarning }">
          📥 {{ downloadsRemaining }} of {{ meta.maxDownloads }} downloads remaining
        </span>
      </div>

      <!-- Expiry info -->
      <div v-if="meta.expiresAt" class="badge-row">
        <span class="badge">
          ⏱️ Expires {{ new Date(meta.expiresAt).toLocaleString() }}
        </span>
      </div>

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
.banner {
  padding: 12px 14px;
  border-radius: 10px;
  margin-bottom: 16px;
  font-weight: 600;
  font-size: 14px;
}
.banner.warning {
  background: #fef3c7;
  color: #92400e;
  border: 1px solid #fcd34d;
}
.badge-row {
  display: flex;
  gap: 8px;
  margin-bottom: 8px;
  flex-wrap: wrap;
}
.badge {
  display: inline-block;
  background: var(--bg);
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 500;
  border: 1px solid var(--border);
}
.badge-warning {
  background: #fef3c7;
  border-color: #fcd34d;
  color: #92400e;
}
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
  margin-top: 16px;
}
.actions .btn {
  text-decoration: none;
}
</style>
