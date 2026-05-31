# SmartCRM AI-Agent

SmartCRM is a customer relationship management system powered by AI. It helps businesses manage customers and automate tasks using artificial intelligence. The system can read product documents to answer customer questions and perform actions like creating leads or sending emails automatically.

## Key Features

- AI Chat: Users can ask questions about products. The AI reads uploaded documents to provide accurate answers.
- AI Agent: The system can understand user requests and perform tasks. For example, it can create a new customer lead or check inventory.
- Background Processing: Heavy tasks are handled in the background to keep the system fast.

## Project Structure

The project is divided into several parts:

- Frontend: A web interface for users to chat and manage data.
- Web API: The central part that handles requests and connects to the AI models.
- Lead Service: A background worker that processes specific tasks like creating leads.
- Data Ingestion: A tool to process and save documents into the system.
- Database: Stores information and document data using PostgreSQL.
- Message Queue: Uses RabbitMQ to pass tasks between services.

## Technologies Used

- .NET 10 for the backend services.
- Vue.js for the frontend website.
- PostgreSQL with pgvector for data and document storage.
- RabbitMQ for communication between services.
- Semantic Kernel to connect with AI models.

## How to Run the Project

### Prerequisites

- Docker Desktop installed on your computer.
- An AI model running locally (like Ollama) or an OpenAI API key.

### Setup Steps

1. Clone or download the project folder.
2. Open a terminal or command prompt in the project root directory.
3. Run the following command to start all services:
   
   docker-compose up -d

4. Once the process is finished, you can access the system at:
   - Web Interface: http://localhost:8080
   - API Documentation: http://localhost:5002

### Processing Documents

To load your product documents into the system, run this command:

docker-compose --profile tools up dataingestion

## Configuration

You can find settings for the database connection and AI model in the docker-compose.yml file under the environment section of each service. By default, it is set to connect to an AI model running on your local machine.
