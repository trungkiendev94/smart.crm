<template>
  <div class="settings-container">
    <header class="section-header">
      <h3>AI Agent Persona & Instructions</h3>
      <p>Fine-tune how your AI behaves, its tone, and its decision-making logic.</p>
    </header>

    <div class="settings-card">
      <div class="form-group">
        <label>System Prompt (The "Brain" Instruction)</label>
        <textarea 
          v-model="systemPrompt" 
          placeholder="Enter instructions for the AI..."
          :disabled="loading"
        ></textarea>
        <p class="help-text">
          Tip: Tell the AI to be concise or follow specific business rules here.
        </p>
      </div>

      <div class="actions">
        <button @click="handleSave" :disabled="loading || !systemPrompt.trim()" class="primary-btn">
          {{ loading ? 'Saving...' : 'Update AI Configuration' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { settingsApi } from '../api';

const systemPrompt = ref('');
const loading = ref(false);

const fetchSettings = async () => {
  loading.value = true;
  try {
    const res = await settingsApi.get('AgentInstructions');
    systemPrompt.value = res.data.value;
  } catch (err: any) {
    if (err.response?.status !== 404) {
      console.error('Error loading settings', err);
    }
  } finally {
    loading.value = false;
  }
};

const handleSave = async () => {
  loading.value = true;
  try {
    await settingsApi.save('AgentInstructions', systemPrompt.value);
    alert('AI Agent instructions updated successfully!');
  } catch (err) {
    alert('Failed to save settings.');
  } finally {
    loading.value = false;
  }
};

onMounted(fetchSettings);
</script>

<style scoped>
.settings-container {
  max-width: 800px;
  margin: 0 auto;
}

.settings-card {
  background: white;
  padding: 2rem;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.05);
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

label {
  font-weight: 700;
  color: #333;
}

textarea {
  min-height: 400px;
  padding: 1rem;
  border: 2px solid #eee;
  border-radius: 10px;
  font-family: 'Fira Code', monospace;
  font-size: 0.9rem;
  line-height: 1.6;
  resize: vertical;
}

textarea:focus {
  border-color: #007bff;
  outline: none;
}

.help-text {
  font-size: 0.8rem;
  color: #888;
  font-style: italic;
}

.actions {
  margin-top: 1.5rem;
  display: flex;
  justify-content: flex-end;
}

.primary-btn {
  background: #007bff;
  color: white;
  border: none;
  padding: 0.8rem 2rem;
  border-radius: 8px;
  font-weight: 700;
  cursor: pointer;
}
</style>
