<template>
  <div class="data-viewer">
    <header class="section-header">
      <h3>Database Direct Viewer</h3>
      <p>Explore all tables and records in your PostgreSQL instance.</p>
    </header>

    <div class="viewer-layout">
      <!-- Table Selector -->
      <aside class="table-list">
        <div class="sidebar-title">Tables</div>
        <ul>
          <li 
            v-for="table in tables" 
            :key="table" 
            :class="{ active: selectedTable === table }"
            @click="selectTable(table)"
          >
            📊 {{ table }}
          </li>
        </ul>
      </aside>

      <!-- Grid Content -->
      <main class="grid-container">
        <div v-if="selectedTable" class="grid-controls">
          <div class="search-bar">
            <select v-model="searchColumn">
              <option value="">Select Column</option>
              <option v-for="col in columns" :key="col" :value="col">{{ col }}</option>
            </select>
            <input 
              v-model="searchQuery" 
              placeholder="Search in selected column..." 
              @keyup.enter="fetchData"
            />
            <button @click="fetchData" class="icon-btn">🔍</button>
          </div>
          <div class="pagination-info">
            Total: {{ totalCount }} records
          </div>
        </div>

        <div v-if="selectedTable" class="table-wrapper">
          <table v-if="!loading">
            <thead>
              <tr>
                <th v-for="col in columns" :key="col">{{ col }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(row, idx) in rows" :key="idx">
                <td v-for="col in columns" :key="col">
                  <div class="cell-content" :title="String(row[col])">
                    {{ formatCell(row[col]) }}
                  </div>
                </td>
              </tr>
              <tr v-if="rows.length === 0">
                <td :colspan="columns.length" class="empty-state">No records found.</td>
              </tr>
            </tbody>
          </table>
          <div v-else class="loading-overlay">Loading data from DB...</div>
        </div>

        <div v-else class="no-selection">
          <div class="placeholder-icon">🗄️</div>
          <p>Please select a table from the left to view its contents.</p>
        </div>

        <!-- Pagination Footer -->
        <footer v-if="selectedTable && totalPages > 1" class="pagination">
          <button :disabled="page <= 1" @click="changePage(page - 1)">Previous</button>
          <span class="page-num">Page {{ page }} of {{ totalPages }}</span>
          <button :disabled="page >= totalPages" @click="changePage(page + 1)">Next</button>
        </footer>
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue';
import { dbApi } from '../api';

const tables = ref<string[]>([]);
const selectedTable = ref('');
const columns = ref<string[]>([]);
const rows = ref<any[]>([]);
const loading = ref(false);

const page = ref(1);
const pageSize = ref(10);
const totalCount = ref(0);
const totalPages = computed(() => Math.ceil(totalCount.value / pageSize.value));

const searchColumn = ref('');
const searchQuery = ref('');

const fetchTables = async () => {
  try {
    const res = await dbApi.getTables();
    tables.value = res.data;
  } catch (err) {
    console.error('Error fetching tables', err);
  }
};

const selectTable = (name: string) => {
  selectedTable.value = name;
  page.value = 1;
  searchColumn.value = '';
  searchQuery.value = '';
  fetchData();
};

const fetchData = async () => {
  if (!selectedTable.value) return;
  loading.value = true;
  try {
    const res = await dbApi.getTableData(
      selectedTable.value, 
      page.value, 
      pageSize.value, 
      searchColumn.value, 
      searchQuery.value
    );
    columns.value = res.data.columns;
    rows.value = res.data.rows;
    totalCount.value = res.data.totalCount;
  } catch (err) {
    alert('Error fetching table data');
  } finally {
    loading.value = false;
  }
};

const changePage = (newPage: number) => {
  page.value = newPage;
  fetchData();
};

const formatCell = (val: any) => {
  if (val === null || val === undefined) return 'NULL';
  if (typeof val === 'object') return JSON.stringify(val).substring(0, 50) + '...';
  return String(val);
};

onMounted(fetchTables);
</script>

<style scoped>
.data-viewer {
  display: flex;
  flex-direction: column;
  height: 100%;
  gap: 1.5rem;
}

.viewer-layout {
  display: grid;
  grid-template-columns: 250px 1fr;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.08);
  flex: 1;
  overflow: hidden;
}

.table-list {
  background: #f8f9fa;
  border-right: 1px solid #eee;
  padding: 1rem 0;
  overflow-y: auto;
}

.sidebar-title {
  padding: 0 1.5rem 1rem;
  font-weight: 800;
  color: #888;
  font-size: 0.75rem;
  text-transform: uppercase;
}

.table-list ul {
  list-style: none;
  padding: 0;
  margin: 0;
}

.table-list li {
  padding: 0.75rem 1.5rem;
  cursor: pointer;
  font-size: 0.9rem;
  transition: all 0.2s;
  color: #444;
}

.table-list li:hover {
  background: #e9ecef;
}

.table-list li.active {
  background: #007bff;
  color: white;
  font-weight: 600;
}

.grid-container {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.grid-controls {
  padding: 1rem 1.5rem;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #fff;
}

.search-bar {
  display: flex;
  gap: 0.5rem;
}

.search-bar select, .search-bar input {
  padding: 0.5rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font-size: 0.85rem;
}

.search-bar input { width: 250px; }

.pagination-info {
  font-size: 0.85rem;
  color: #666;
}

.table-wrapper {
  flex: 1;
  overflow: auto;
  position: relative;
}

table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.85rem;
}

th {
  position: sticky;
  top: 0;
  background: #f8f9fa;
  padding: 12px;
  text-align: left;
  border-bottom: 2px solid #dee2e6;
  color: #555;
  white-space: nowrap;
}

td {
  padding: 10px 12px;
  border-bottom: 1px solid #eee;
  max-width: 300px;
}

.cell-content {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.loading-overlay {
  padding: 3rem;
  text-align: center;
  color: #888;
  font-style: italic;
}

.no-selection {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  color: #bbb;
}

.placeholder-icon {
  font-size: 4rem;
  margin-bottom: 1rem;
}

.pagination {
  padding: 1rem;
  border-top: 1px solid #eee;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 1rem;
  background: #f8f9fa;
}

.pagination button {
  padding: 0.4rem 1rem;
  border: 1px solid #ddd;
  background: white;
  border-radius: 4px;
  cursor: pointer;
}

.pagination button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.page-num { font-size: 0.85rem; font-weight: 600; }

.icon-btn {
  background: #007bff;
  color: white;
  border: none;
  padding: 0.5rem 0.8rem;
  border-radius: 6px;
  cursor: pointer;
}
</style>
