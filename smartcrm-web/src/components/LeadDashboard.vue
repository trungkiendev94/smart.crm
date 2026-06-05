<template>
  <div class="lead-dashboard">
    <div class="leads-section">
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

    <!-- Email History Section -->
    <div class="email-history">
      <div class="header">
        <h3>Email Sending History</h3>
        <button @click="fetchEmailHistory" class="refresh-btn">Refresh History</button>
      </div>
      <div class="table-container">
        <table>
          <thead>
            <tr>
              <th>Recipient</th>
              <th>Template</th>
              <th>Content Snippet</th>
              <th>Sent At</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="email in emailHistory" :key="email.id">
              <td>{{ email.recipientEmail }}</td>
              <td><span class="template-chip">{{ email.template }}</span></td>
              <td class="content-cell" :title="email.content">{{ email.content }}</td>
              <td>{{ new Date(email.sentAt).toLocaleString() }}</td>
            </tr>
            <tr v-if="emailHistory.length === 0">
              <td colspan="4" class="no-data">No emails sent yet.</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { customerApi, emailsApi } from '../api';

const leads = ref<any[]>([]);
const emailHistory = ref<any[]>([]);

const fetchLeads = async () => {
  try {
    const response = await customerApi.getCustomers();
    leads.value = response.data;
  } catch (error) {
    console.error('Failed to fetch leads:', error);
  }
};

const fetchEmailHistory = async () => {
  try {
    const response = await emailsApi.getHistory();
    emailHistory.value = response.data;
  } catch (error) {
    console.error('Failed to fetch email history:', error);
  }
};

onMounted(() => {
  fetchLeads();
  fetchEmailHistory();
});
</script>

<style scoped>
.lead-dashboard {
  display: flex;
  flex-direction: column;
  gap: 2rem;
  background: white;
  border-radius: 12px;
  padding: 1.5rem;
  box-shadow: 0 4px 20px rgba(0,0,0,0.08);
}

.email-history {
  border-top: 1px solid #eee;
  padding-top: 1.5rem;
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

.status-chip, .template-chip {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 600;
}

.status-chip {
  background: #e8f5e9;
  color: #2e7d32;
}

.template-chip {
  background: #e3f2fd;
  color: #1565c0;
}

.content-cell {
  max-width: 300px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-size: 0.9rem;
  color: #666;
}

.no-data {
  text-align: center;
  padding: 2rem;
  color: #888;
}
</style>
