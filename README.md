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
- ✅ Testes unitários completos

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
Edite os arquivos **appsettings.json** em _ReportGenerator.Api_ e _ReportGenerator.Worker_:

### 3. Inicie o RabbitMQ
```bash
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```
