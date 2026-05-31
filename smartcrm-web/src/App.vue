<template>
  <div class="app-wrapper">
    <nav class="sidebar">
      <div class="logo">SmartCRM AI</div>
      <ul class="nav-links">
        <li :class="{ active: currentTab === 'chat' }" @click="currentTab = 'chat'">AI Agent Chat</li>
        <li :class="{ active: currentTab === 'knowledge' }" @click="currentTab = 'knowledge'">Training Memory</li>
        <li :class="{ active: currentTab === 'settings' }" @click="currentTab = 'settings'">Agent Settings</li>
        <li :class="{ active: currentTab === 'database' }" @click="currentTab = 'database'">Data Viewer</li>
      </ul>
    </nav>
    
    <main class="main-content">
      <header class="top-bar">
        <h1>Internal Dashboard</h1>
        <div class="user-info">Autonomous Agent Mode</div>
      </header>
      
      <div class="dashboard-body">
        <div v-if="currentTab === 'chat'" class="dashboard-grid">
          <section class="chat-section">
            <SmartChat />
          </section>
          
          <section class="data-section">
            <LeadDashboard />
          </section>
        </div>

        <div v-if="currentTab === 'knowledge'" class="full-view">
          <KnowledgeManagement />
        </div>

        <div v-if="currentTab === 'settings'" class="full-view">
          <AgentSettings />
        </div>

        <div v-if="currentTab === 'database'" class="full-view">
          <DataViewer />
        </div>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import SmartChat from './components/SmartChat.vue';
import LeadDashboard from './components/LeadDashboard.vue';
import KnowledgeManagement from './components/KnowledgeManagement.vue';
import AgentSettings from './components/AgentSettings.vue';
import DataViewer from './components/DataViewer.vue';

const currentTab = ref<'chat' | 'knowledge' | 'settings' | 'database'>('chat');
</script>

<style>
:root {
  --primary-color: #007bff;
  --bg-color: #f0f2f5;
  --sidebar-color: #1a2a3a;
}

body {
  margin: 0;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, sans-serif;
  background-color: var(--bg-color);
}

.app-wrapper {
  display: flex;
  height: 100vh;
}

.sidebar {
  width: 240px;
  background: var(--sidebar-color);
  color: white;
  padding: 2rem 0;
}

.logo {
  font-size: 1.5rem;
  font-weight: 800;
  padding: 0 2rem;
  margin-bottom: 3rem;
  background: linear-gradient(45deg, #007bff, #00d4ff);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}

.nav-links {
  list-style: none;
  padding: 0;
}

.nav-links li {
  padding: 1rem 2rem;
  cursor: pointer;
  transition: background 0.2s;
  border-left: 4px solid transparent;
}

.nav-links li.active {
  background: rgba(255,255,255,0.1);
  border-left-color: var(--primary-color);
}

.main-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.top-bar {
  background: white;
  padding: 1rem 2rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #ddd;
}

.top-bar h1 {
  font-size: 1.25rem;
  margin: 0;
}

.dashboard-body {
  flex: 1;
  overflow: hidden;
  padding: 2rem;
}

.dashboard-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 2rem;
  height: 100%;
}

.full-view {
  height: 100%;
  overflow-y: auto;
}

.chat-section, .data-section {
  height: 100%;
  overflow: hidden;
}

@media (max-width: 1200px) {
  .dashboard-grid {
    grid-template-columns: 1fr;
    overflow-y: auto;
  }
  .chat-section {
    height: 600px;
  }
}
</style>
