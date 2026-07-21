<script setup>
defineProps({
  percent: { type: Number, default: 0 },
  label: { type: String, default: '' },
})
</script>

<template>
  <div class="progress">
    <div class="track">
      <div class="fill" :style="{ width: `${percent}%` }"></div>
    </div>
    <div class="meta">
      <span class="muted">{{ label }}</span>
      <span class="pct">{{ Math.round(percent) }}%</span>
    </div>
  </div>
</template>

<style scoped>
.track {
  height: 8px;
  width: 100%;
  background: var(--bg);
  border-radius: 999px;
  overflow: hidden;
}
.fill {
  height: 100%;
  background: var(--primary);
  border-radius: 999px;
  transition: width 0.2s ease;
}
.meta {
  display: flex;
  justify-content: space-between;
  margin-top: 6px;
  font-size: 13px;
}
.pct {
  font-weight: 700;
}
</style>
