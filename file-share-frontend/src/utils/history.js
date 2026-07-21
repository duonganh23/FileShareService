// Local upload history persisted in the browser. The backend has no per-user
// concept, so "your uploads" are tracked client-side in localStorage.

const KEY = 'fileshare.history'

export function getHistory() {
  try {
    return JSON.parse(localStorage.getItem(KEY) || '[]')
  } catch {
    return []
  }
}

export function addToHistory(entry) {
  const list = getHistory()
  // De-dupe by code, newest first.
  const next = [entry, ...list.filter((e) => e.code !== entry.code)]
  localStorage.setItem(KEY, JSON.stringify(next))
  return next
}

export function removeFromHistory(code) {
  const next = getHistory().filter((e) => e.code !== code)
  localStorage.setItem(KEY, JSON.stringify(next))
  return next
}
