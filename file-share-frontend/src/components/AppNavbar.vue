<script setup>
import { RouterLink } from 'vue-router'
</script>

<template>
  <header class="navbar">
    <div class="navbar-inner">
      <RouterLink to="/" class="brand">SecureShare</RouterLink>
      <nav class="links">
        <RouterLink to="/" class="link">Upload</RouterLink>
        <RouterLink to="/history" class="link">History</RouterLink>
      </nav>
    </div>
  </header>
</template>

<style scoped>
.navbar {
  background: var(--surface);
  border-bottom: 1px solid var(--border);
}
.navbar-inner {
  max-width: 960px;
  margin: 0 auto;
  padding: 0 20px;
  height: 60px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.brand {
  font-size: 20px;
  font-weight: 800;
  text-decoration: none;
  letter-spacing: -0.02em;
}
.links {
  display: flex;
  gap: 8px;
}
.link {
  text-decoration: none;
  padding: 8px 14px;
  border-radius: 8px;
  font-weight: 600;
  color: var(--muted);
}
.link:hover {
  background: var(--bg);
}
.link.router-link-active {
  color: var(--text);
  background: var(--bg);
}
</style>
