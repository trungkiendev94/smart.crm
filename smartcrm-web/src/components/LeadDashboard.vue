<template>
  <div class="lead-dashboard">
    <div class="header">
      <h3>CRM Lead Management</h3>
      <button @click="fetchLeads" class="refresh-btn">Refresh Leads</button>
    </div>

    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Full Name</th>
            <th>Email</th>
            <th>Status</th>
            <th>Created At</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="lead in leads" :key="lead.id">
            <td>{{ lead.fullName }}</td>
            <td>{{ lead.email }}</td>
            <td><span class="status-chip">{{ lead.status }}</span></td>
            <td>{{ new Date(lead.createdAt).toLocaleString() }}</td>
          </tr>
          <tr v-if="leads.length === 0">
            <td colspan="4" class="no-data">No leads found. Use the AI Assistant to create some!</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { customerApi } from '../api';

const leads = ref<any[]>([]);

const fetchLeads = async () => {
  try {
    const response = await customerApi.getCustomers();
    leads.value = response.data;
  } catch (error) {
    console.error('Failed to fetch leads:', error);
  }
};

onMounted(fetchLeads);
</script>

<style scoped>
.lead-dashboard {
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 4px 20px rgba(0,0,0,0.08);
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 1.5rem;
}

.header h3 {
  margin: 0;
  color: #2c3e50;
}

.refresh-btn {
  background: #f8f9fa;
  border: 1px solid #ddd;
  padding: 6px 12px;
  border-radius: 6px;
  cursor: pointer;
  font-size: 0.85rem;
}

.table-container {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
}

th {
  background: #f8f9fa;
  padding: 12px;
  font-weight: 600;
  color: #666;
  border-bottom: 2px solid #eee;
}

td {
  padding: 12px;
  border-bottom: 1px solid #eee;
  color: #333;
}

.status-chip {
  background: #e8f5e9;
  color: #2e7d32;
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 600;
}

.no-data {
  text-align: center;
  padding: 2rem;
  color: #888;
}
</style>
