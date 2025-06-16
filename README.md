# IBRReportGenerator
Desafio Técnico – API de Geração de Relatórios em PDF com RabbitMQ

Geração assíncrona de relatórios em PDF, utilizando RabbitMQ para processamento em fila, QuestPDF para geração de documentos e webhooks para entrega de resultados. Desenvolvido com .NET 8, seguindo princípios de Clean Code e injeção de dependência.

## 📋 Sobre o Projeto

**IBRReportGenerator** é uma solução robusta para geração de relatórios em PDF de forma assíncrona. A aplicação consiste em uma API RESTful que recebe solicitações de relatórios, um worker que processa essas solicitações via RabbitMQ, e um sistema de entrega de resultados via webhooks. O projeto suporta parâmetros flexíveis, incluindo dicionários, objetos e imagens em Base64, com validações rigorosas e testes unitários completos.

## 🏗️ Arquitetura

O projeto segue uma arquitetura modular, dividida em camadas claras:
```
src/
├── ReportGenerator.Api/         # API RESTful (Controllers, Swagger) 
├── ReportGenerator.Worker/      # Worker para processamento de filas
├── ReportGenerator.Domain/      # Modelos e contratos compartilhados
└── ReportGenerator.Tests/       # Testes unitários (xUnit, Moq)
```

### 🎯 Funcionalidades Implementadas

#### 📄 Geração de Relatórios
- ✅ Criar relatórios em PDF via endpoint POST
- ✅ Suporte a parâmetros genéricos (`object`)
- ✅ Renderização de dicionários e listas
- ✅ Suporte a imagens em Base64
- ✅ Formato A4 com cabeçalho e rodapé

#### 🐰 Processamento Assíncrono
- ✅ Enfileiramento de solicitações com RabbitMQ
- ✅ Processamento em background pelo Worker
- ✅ Confirmação manual de mensagens (Ack/Nack)

#### 📡 Entrega de Resultados
- ✅ Envio de PDFs via webhooks
- ✅ Payload com status, mensagem e PDF em Base64
- ✅ Tratamento de erros com requeue

#### 🛡️ Robustez
- ✅ Validações de entrada e parâmetros
- ✅ Logging estruturado
- ✅ Testes unitários

## 🛠️ Tecnologias Utilizadas

### Backend & Framework
- **.NET 8.0** - Framework principal
- **C#** - Linguagem de programação
- **ASP.NET Core** - API Web framework

### Mensageria
- **RabbitMQ** - Fila de mensagens (versão 6.8 via Client)
- **RabbitMQ.Client** - Biblioteca para integração

### Geração de PDF
- **QuestPDF** - Biblioteca para geração de PDFs dinâmicos

### Testes
- **xUnit** - Framework de testes
- **Moq** - Biblioteca para mocks
- **Shouldly** - Asserções fluentes (se aplicável)

### DevOps & Ferramentas
- **Docker** - Containerização do RabbitMQ
- **Swagger/OpenAPI** - Documentação automática da API
- **Microsoft.Extensions.Logging** - Logging nativo

### Padrões Arquiteturais
- **Clean Code** - Código limpo e organizado
- **Dependency Injection** - Inversão de controle
- **Repository Pattern** - Abstração de mensageria
- **Result Pattern** - Tratamento de erros (parcial)

## 📋 Pré-requisitos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download) ou superior
- [Docker](https://www.docker.com/) (para RabbitMQ)
- [Git](https://git-scm.com/)

## 🔧 Instalação e Configuração

### 1. Clone o repositório
```bash
git clone https://github.com/mllacerda/IBRReportGenerator.git
cd IBRReportGenerator
```
### 2. Configure as connection strings
Edite os arquivos _``appsettings.json``_ em ``ReportGenerator.Api_`` e ``ReportGenerator.Worker``:

### 3. Inicie o RabbitMQ
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 4. Execute a API
```bash
cd src/ReportGenerator.Api
dotnet restore
dotnet run
```

### 5. Execute o Worker
```bash
cd src/ReportGenerator.Worker
dotnet restore
dotnet run
```

### 6. Acesse a aplicação
- **API**: `https://localhost:<porta>` ou `http://localhost:<porta>`
- **Swagger UI**: `https://localhost:<porta>/swagger`

> **Nota**: Verifique as portas padrão nos arquivos `launchSettings.json`.

## 📁 Estrutura Detalhada do Projeto

```
ReportGen/
├── src/
│   ├── ReportGenerator.Api/
│   │   ├── Controllers/                # Controllers da API
│   │   ├── Infrastructure/Messaging/    # Integração com RabbitMQ
│   │   ├── Properties/                 # Configurações de launch
│   │   ├── Program.cs                  # Configuração da aplicação
│   │   └── appsettings.json            # Configurações
│   │
│   ├── ReportGenerator.Worker/
│   │   ├── Services/                   # Serviços (PDF, Webhook)
│   │   ├── ReportWorker.cs             # Worker do RabbitMQ
│   │   ├── Program.cs                  # Configuração do Worker
│   │   └── appsettings.json            # Configurações
│   │
│   ├── ReportGenerator.Domain/
│   │   ├── Models/                     # Modelos (ReportRequest, RabbitMQSettings)
│   │   └── Interfaces/                 # Contratos (IMessageQueueService)
│   │
│   └── ReportGenerator.Tests/
│       ├── TestConstants.cs            # Constantes para testes
│       ├── RabbitMQServiceTests.cs     # Testes do RabbitMQ
│       ├── ReportGeneratorServiceTests.cs # Testes do PDF
│       ├── ReportWorkerTests.cs        # Testes do Worker
│       └── WebhookServiceTests.cs      # Testes do Webhook
```

## 🧪 Testes

Para executar os testes:

```bash
cd tests/ReportGenerator.Tests
dotnet test
```

## 📚 API Endpoints

### 📄 Geração de Relatórios

| Método | Endpoint | Descrição | Parâmetros |
|--------|----------|-----------|------------|
| `POST` | `/api/reports` | Solicita a geração de um relatório | Body: `ReportRequest` |

### 📋 Exemplos de Uso

#### Criar um relatório:
```json
POST /api/reports
{
  "reportId": "test-123",
  "webhookUrl": "https://webhook.site/abc123",
  "parameters": {
    "key1": "value1",
    "key2": 42
  }
}
```

#### Com imagem Base64:
```json
POST /api/reports
{
  "reportId": "test-124",
  "webhookUrl": "https://webhook.site/abc123",
  "parameters": "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8DwHwAFBQIAX8cX3QAAAABJRU5ErkJggg=="
}
```

## 🔧 Configurações Avançadas

### Logging
- Microsoft.Extensions.Logging para logging estruturado.
- Logs de processamento, erros e webhooks.
- Configuração via `appsettings.json`

### Validações
- Validação de ``ReportRequest`` (não nulo).
- Suporte a parâmetros genéricos (``object``).
- Tratamento de erros com mensagens claras.

### Performance
- RabbitMQ para processamento assíncrono.
- QuestPDF para geração eficiente de PDFs.
- Confirmação manual de mensagens (Ack/Nack).

## 🔄 Ideias - Próximas Funcionalidades

- [ ] Reconexão automática no RabbitMQ
- [ ] Suporte a múltiplas filas
- [ ] Configuração do formato do PDF via ``appsettings.json``
- [ ] Cache de relatórios gerados
- [ ] Autenticação na API

---
