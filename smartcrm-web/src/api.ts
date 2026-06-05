import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5002/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

export const chatApi = {
  sendMessage: (message: string) => api.post('/chat', { message }),
};

export const customerApi = {
  getCustomers: () => api.get('/customers'),
};

export const knowledgeApi = {
  ingest: (title: string, content: string) => api.post('/knowledge/ingest', { title, content }),
  update: (id: string, title: string, content: string) => api.put(`/knowledge/${id}`, { title, content }),
  delete: (id: string) => api.delete(`/knowledge/${id}`),
  getList: () => api.get('/knowledge'),
};

export const settingsApi = {
  get: (key: string) => api.get(`/settings/${key}`),
  save: (key: string, value: string) => api.post('/settings', { key, value }),
};

export const dbApi = {
  getTables: () => api.get('/database/tables'),
  getTableData: (tableName: string, page = 1, pageSize = 10, searchColumn = '', searchQuery = '') => 
    api.get(`/database/data/${tableName}`, {
      params: { page, pageSize, searchColumn, searchQuery }
    }),
};

export const emailsApi = {
  getHistory: () => api.get('/emails')
};

export default api;
