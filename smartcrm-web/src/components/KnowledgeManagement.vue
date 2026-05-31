<template>
  <div class="knowledge-container">
    <header class="section-header">
      <h3>AI Knowledge Management</h3>
      <p>Configure the "brain" of your CRM AI Agent.</p>
    </header>

    <div class="main-layout">
      <!-- Edit/Add Form -->
      <div class="ingest-form">
        <div class="form-title">
          {{ selectedId ? 'Edit Knowledge Record' : 'Add New Knowledge' }}
          <button v-if="selectedId" @click="resetForm" class="text-link">Cancel</button>
        </div>
        <div class="form-group">
          <label>Title</label>
          <input v-model="formData.title" placeholder="e.g., Summer Promotion 2026" />
        </div>
        <div class="form-group">
          <label>Content (Context for AI)</label>
          <textarea v-model="formData.content" placeholder="Describe the product or policy..."></textarea>
        </div>
        <div class="actions">
          <button @click="handleSave" :disabled="loading || !formData.content.trim()" class="primary-btn">
            {{ loading ? 'Processing...' : (selectedId ? 'Update & Re-vectorize' : 'Vectorize & Save') }}
          </button>
          <button v-if="selectedId" @click="handleDelete" class="delete-btn">Delete</button>
        </div>
      </div>

      <!-- List for selection -->
      <div class="knowledge-list">
        <h4>Stored Intelligence</h4>
        <div class="table-card">
          <table>
            <thead>
              <tr>
                <th>Title</th>
                <th>Source</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in knowledgeList" :key="item.id" :class="{ selected: selectedId === item.id }">
                <td>{{ item.title }}</td>
                <td>{{ item.source }}</td>
                <td>
                  <button @click="selectItem(item)" class="edit-link">Select to Edit</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { knowledgeApi } from '../api';

const loading = ref(false);
const knowledgeList = ref<any[]>([]);
const selectedId = ref<string | null>(null);
const formData = reactive({
  title: '',
  content: ''
});

const fetchKnowledge = async () => {
  try {
    const res = await knowledgeApi.getList();
    knowledgeList.value = res.data;
  } catch (err) {
    console.error('Failed to load knowledge', err);
  }
};

const selectItem = (item: any) => {
  selectedId.value = item.id;
  formData.title = item.title;
  formData.content = item.content;
};

const resetForm = () => {
  selectedId.value = null;
  formData.title = '';
  formData.content = '';
};

const handleSave = async () => {
  if (!formData.content.trim() || loading.value) return;
  
  loading.value = true;
  try {
    if (selectedId.value) {
      await knowledgeApi.update(selectedId.value, formData.title, formData.content);
    } else {
      await knowledgeApi.ingest(formData.title, formData.content);
    }
    alert('AI Knowledge updated successfully!');
    resetForm();
    await fetchKnowledge();
  } catch (err) {
    alert('Error saving knowledge.');
  } finally {
    loading.value = false;
  }
};

const handleDelete = async () => {
  if (!selectedId.value || !confirm('Are you sure you want to delete this knowledge?')) return;
  
  loading.value = true;
  try {
    await knowledgeApi.delete(selectedId.value);
    resetForm();
    await fetchKnowledge();
  } finally {
    loading.value = false;
  }
};

onMounted(fetchKnowledge);
</script>

<style scoped>
.knowledge-container {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.main-layout {
  display: grid;
  grid-template-columns: 400px 1fr;
  gap: 2rem;
  align-items: start;
}

.ingest-form {
  background: white;
  padding: 1.5rem;
  border-radius: 12px;
  box-shadow: 0 4px 15px rgba(0,0,0,0.05);
  position: sticky;
  top: 0;
}

.form-title {
  font-weight: 800;
  margin-bottom: 1.5rem;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.form-group {
  margin-bottom: 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

label { font-weight: 600; font-size: 0.85rem; color: #666; }

input, textarea {
  padding: 0.75rem;
  border: 1px solid #ddd;
  border-radius: 8px;
  font-family: inherit;
}

textarea { min-height: 200px; resize: vertical; }

.actions {
  display: flex;
  gap: 0.5rem;
  margin-top: 1rem;
}

.primary-btn {
  flex: 1;
  background: #007bff;
  color: white;
  border: none;
  padding: 0.8rem;
  border-radius: 8px;
  font-weight: 700;
  cursor: pointer;
}

.delete-btn {
  background: #fee2e2;
  color: #dc2626;
  border: 1px solid #fecaca;
  padding: 0.8rem;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
}

.knowledge-list table {
  width: 100%;
  border-collapse: collapse;
}

.table-card {
  background: white;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 4px 15px rgba(0,0,0,0.05);
}

th, td { padding: 12px 15px; text-align: left; border-bottom: 1px solid #eee; }
th { background: #f8f9fa; color: #888; font-size: 0.75rem; text-transform: uppercase; }

tr.selected { background: #e3f2fd; }

.edit-link {
  background: none;
  border: none;
  color: #007bff;
  text-decoration: underline;
  cursor: pointer;
  padding: 0;
  font-size: 0.9rem;
}

.text-link {
  background: none;
  border: none;
  color: #666;
  cursor: pointer;
  font-size: 0.8rem;
}
</style>
