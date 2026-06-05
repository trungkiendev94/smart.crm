-- Enable pgvector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- Create Customers table
CREATE TABLE IF NOT EXISTS "Customers" (
    "Id" UUID PRIMARY KEY,
    "FullName" TEXT NOT NULL,
    "Email" TEXT NOT NULL,
    "Phone" TEXT,
    "Status" TEXT NOT NULL DEFAULT 'Lead',
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

-- Create KnowledgeBases table
CREATE TABLE IF NOT EXISTS "KnowledgeBases" (
    "Id" UUID PRIMARY KEY,
    "Title" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "Embedding" vector(384),
    "Source" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

-- Create SentEmails table
CREATE TABLE IF NOT EXISTS "SentEmails" (
    "Id" UUID PRIMARY KEY,
    "RecipientEmail" TEXT NOT NULL,
    "Template" TEXT NOT NULL,
    "Content" TEXT NOT NULL,
    "SentAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

-- Create SystemSettings table
CREATE TABLE IF NOT EXISTS "SystemSettings" (
    "Id" UUID PRIMARY KEY,
    "Key" TEXT NOT NULL UNIQUE,
    "Value" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE
);

-- Seed Default System Settings
INSERT INTO "SystemSettings" ("Id", "Key", "Value", "CreatedAt")
VALUES 
(gen_random_uuid(), 'AgentInstructions', '# SMARTCRM AI ASSISTANT INSTRUCTION

## ROLE
You are a professional CRM Consultant. You provide accurate info and help manage leads.

## LANGUAGE RULE
- **VIETNAMESE & ENGLISH**: Respond in the same language the user uses. Prefer Vietnamese if unsure.', NOW())
ON CONFLICT ("Key") DO NOTHING;

INSERT INTO "SystemSettings" ("Id", "Key", "Value", "CreatedAt")
VALUES 
(gen_random_uuid(), 'RerankPrompt', 'You are an intelligent Reranking system. Your task is to evaluate the relevance of documents (DOC) to a question (QUERY).

QUERY: {query}

DOCUMENT LIST:
{documents}

REQUIREMENTS:
1. Analyze the QUERY and each document.
2. Select up to {topK} most useful documents to answer the QUERY.
3. Return the results as a JSON array containing the document IDs in descending order of importance.
4. ONLY RETURN THE JSON ARRAY. Example: [2, 0, 5]

RESULT (JSON ARRAY):', NOW())
ON CONFLICT ("Key") DO NOTHING;
