import axios from 'axios'

const baseURL = import.meta.env.VITE_API_BASE_URL || '/api'

const http = axios.create({ baseURL })


export const filesApi = {
  /**
   * Upload a single file.
   * @param {File} file
   * @param {{ maxDownloads?: number, expiresAt?: string|null }} options
   * @param {(evt: ProgressEvent) => void} [onUploadProgress] axios progress callback
   * @returns metadata: { code, originalFileName, mimeType, sizeBytes, maxDownloads, expiresAt, createdAt }
   */
  async upload(file, { maxDownloads = 0, expiresAt = null } = {}, onUploadProgress) {
    const form = new FormData()
    form.append('file', file)
    form.append('maxDownloads', String(maxDownloads))
    if (expiresAt) form.append('expiresAt', expiresAt)

    const { data } = await http.post('/files', form, {
      headers: { 'Content-Type': 'multipart/form-data' },
      onUploadProgress,
    })
    return data
  },

  /** Fetch metadata for one file. Throws with response.status 404/410 on error. */
  async getInfo(code) {
    const { data } = await http.get(`/files/${code}/info`)
    return data
  },

  /** Absolute URL of the raw download/preview stream (for <img src> or an <a> href). */
  downloadUrl(code) {
    return `${baseURL}/files/${code}`
  },

  /** Delete a file by code. */
  async remove(code) {
    await http.delete(`/files/${code}`)
  },
}

export default filesApi
