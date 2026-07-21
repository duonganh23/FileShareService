import { createRouter, createWebHistory } from 'vue-router'
import UploadView from '@/views/UploadView.vue'
import PreviewView from '@/views/PreviewView.vue'
import HistoryView from '@/views/HistoryView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'upload', component: UploadView },
    { path: '/f/:code', name: 'preview', component: PreviewView, props: true },
    { path: '/history', name: 'history', component: HistoryView },
  ],
})

export default router
