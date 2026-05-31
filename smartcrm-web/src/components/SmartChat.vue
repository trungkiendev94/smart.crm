<template>
  <div class="chat-container">
    <div class="chat-header">
      <h3>SmartCRM AI Assistant</h3>
      <span class="status-badge">Autonomous Agent</span>
    </div>
    
    <div class="chat-messages" ref="scrollContainer">
      <div v-for="(msg, index) in messages" :key="index" :class="['message-wrapper', msg.role]">
        <div class="message">
          <div class="message-content">
            <p v-html="formatText(msg.text)"></p>
          </div>
          <!-- Show tools executed for this assistant message -->
          <div v-if="msg.tools?.length" class="tool-tags">
            <span v-for="tool in msg.tools" :key="tool" class="tool-tag">
              ⚙️ Executed: {{ tool }}
            </span>
          </div>
        </div>
      </div>
      <div v-if="loading" class="message-wrapper assistant loading">
        <div class="message">
          <div class="message-content">
            <p>{{ currentStatus }}</p>
          </div>
        </div>
      </div>
    </div>

    <div class="chat-input-area">
      <input 
        ref="inputField"
        v-model="userInput" 
        @keyup.enter="handleSend" 
        placeholder="Ask anything..." 
        :disabled="loading"
      />
      <button @click="handleSend" :disabled="loading || !userInput.trim()">
        Send
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, nextTick, onMounted } from 'vue';
import { chatApi } from '../api';

const userInput = ref('');
const messages = ref<{ role: 'user' | 'assistant', text: string, tools?: string[] }[]>([
  { role: 'assistant', text: 'Hello! I am your SmartCRM Agent. How can I help you today? 👋' }
]);
const loading = ref(false);
const currentStatus = ref('AI is thinking...');
const scrollContainer = ref<HTMLElement | null>(null);
const inputField = ref<HTMLInputElement | null>(null);

const scrollToBottom = async () => {
  await nextTick();
  if (scrollContainer.value) {
    scrollContainer.value.scrollTop = scrollContainer.value.scrollHeight;
  }
};

const formatText = (text: string) => {
  return text.replace(/\n/g, '<br>');
};

const handleSend = async () => {
  if (!userInput.value.trim() || loading.value) return;

  const text = userInput.value;
  messages.value.push({ role: 'user', text });
  userInput.value = '';
  loading.value = true;
  currentStatus.value = 'AI is thinking...';
  await scrollToBottom();

  try {
    const response = await chatApi.sendMessage(text);
    messages.value.push({ 
      role: 'assistant', 
      text: response.data.reply,
      tools: response.data.executedTools 
    });
  } catch (error: any) {
    messages.value.push({ 
      role: 'assistant', 
      text: 'Error: Connection lost. Is WebAPI running? 🛠️' 
    });
  } finally {
    loading.value = false;
    await scrollToBottom();
    // FIX: Safer auto focus back
    setTimeout(() => {
      inputField.value?.focus();
    }, 50);
  }
  };


onMounted(() => {
  inputField.value?.focus();
});
</script>

<style scoped>
.chat-container {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: white;
  border-radius: 12px;
  box-shadow: 0 4px 20px rgba(0,0,0,0.08);
  overflow: hidden;
}

.chat-header {
  padding: 1rem 1.5rem;
  background: #f8f9fa;
  border-bottom: 1px solid #eee;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.chat-header h3 {
  margin: 0;
  color: #2c3e50;
}

.status-badge {
  font-size: 0.75rem;
  background: #e1f5fe;
  color: #0288d1;
  padding: 4px 10px;
  border-radius: 20px;
  font-weight: 700;
}

.chat-messages {
  flex: 1;
  padding: 1.5rem;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.message-wrapper {
  display: flex;
  flex-direction: column;
}

.message {
  max-width: 85%;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.user { align-items: flex-end; }
.assistant { align-items: flex-start; }

.message-content {
  padding: 0.8rem 1.2rem;
  border-radius: 18px;
  font-size: 0.95rem;
  line-height: 1.6;
}

.user .message-content {
  background: #007bff;
  color: white;
  border-bottom-right-radius: 4px;
}

.assistant .message-content {
  background: #f1f3f5;
  color: #333;
  border-bottom-left-radius: 4px;
}

.tool-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
  margin-top: 0.2rem;
}

.tool-tag {
  font-size: 0.7rem;
  background: #fff3e0;
  color: #e65100;
  border: 1px solid #ffe0b2;
  padding: 2px 8px;
  border-radius: 4px;
  font-family: monospace;
}

.loading .message-content {
  font-style: italic;
  color: #888;
  animation: pulse 1.5s infinite;
}

@keyframes pulse {
  0% { opacity: 0.6; }
  50% { opacity: 1; }
  100% { opacity: 0.6; }
}

.chat-input-area {
  padding: 1rem 1.5rem;
  border-top: 1px solid #eee;
  display: flex;
  gap: 0.75rem;
}

input {
  flex: 1;
  padding: 0.8rem 1.2rem;
  border: 2px solid #eee;
  border-radius: 10px;
  outline: none;
  transition: border-color 0.2s;
}

input:focus {
  border-color: #007bff;
}

button {
  padding: 0 1.5rem;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 10px;
  cursor: pointer;
  font-weight: 700;
}

button:disabled {
  background: #ccc;
  cursor: not-allowed;
}
</style>
