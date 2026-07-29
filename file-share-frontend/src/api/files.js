import axios from 'axios'

const baseURL = import.meta.env.VITE_API_BASE_URL || ''

const http = axios.create({ baseURL })


export const filesApi = {
  /**
   * Upload a single file.
   * @param {File} file
   * @param {{ maxDownloads?: number, expiresAt?: string|null }} options
   * @param {(evt: ProgressEvent) => void} [onUploadProgress] axios progress callback
   * @returns metadata: { code, originalFileName, mimeType, sizeBytes, maxDownloads, expiresAt, createdAt }
   */
   async upload(file, { maxDownloads = 0, expiresAt = null, password = null } = {}, onUploadProgress) {
       const form = new FormData()
       form.append('file', file)
       form.append('maxDownloads', String(maxDownloads))
       if (expiresAt) form.append('expiresAt', expiresAt)
       if (password) form.append('password', password)
       const { data } = await http.post('/files', form, {
           headers: { 'Content-Type': 'multipart/form-data' },
           onUploadProgress,
       })
       return data
   },

  /** Fetch metadata for one file. Throws with response.status 404/410 on error. */
    async getInfo(code, password = null) {
        const params = password ? { password } : {}
        const { data } = await http.get(`/files/${code}/info`, { params })
        return data
    },

  /** Download file as blob (for password-protected files or to avoid exposing password in URLs). */
  async downloadBlob(code, password = null) {
    const config = password ? { headers: { Authorization: `Bearer ${password}` } } : {}
    const { data } = await http.get(`/files/${code}`, { ...config, responseType: 'blob' })
    return data
  },

  /** Absolute URL of the raw download/preview stream (for <img src> or plain <a> href, no password). */
  downloadUrl(code) {
    return `${baseURL}/files/${code}`
  },

  /** Delete a file by code. */
  async remove(code) {
    await http.delete(`/files/${code}`)
  },
}

export default filesApi
