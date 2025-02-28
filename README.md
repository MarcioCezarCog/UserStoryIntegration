# UserStory Integration API

API RESTful para interface de chat personalizado que consome modelos de linguagem (LLM).

## Visão Geral

Este projeto implementa uma API backend em C# usando .NET 9 que consome diferentes serviços de LLM (como OpenAI, Anthropic e Google AI) e expõe endpoints para que uma interface de chat personalizada possa interagir com esses modelos.

## Arquitetura

O projeto segue uma arquitetura limpa (Clean Architecture) com as seguintes camadas:

- **API**: Controladores, filtros, middlewares e configuração da aplicação
- **Application**: DTOs, interfaces, serviços de aplicação e casos de uso
- **Domain**: Entidades de domínio, interfaces de repositório e regras de negócio
- **Infrastructure**: Implementações de repositório, adaptadores para serviços externos (LLM) e acesso a dados

## Tecnologias

- **.NET 9**: Framework principal
- **Entity Framework Core**: ORM para acesso a dados
- **Swagger/OpenAPI**: Documentação da API
- **Serilog**: Logging
- **MediatR**: Implementação do padrão mediator para CQRS
- **AutoMapper**: Mapeamento entre objetos
- **FluentValidation**: Validação de dados de entrada
- **Polly**: Políticas de resiliência para chamadas HTTP
- **Health Checks**: Monitoramento da saúde da aplicação e dependências

## Recursos Principais

- Integração com múltiplos provedores de LLM (OpenAI, Anthropic, Google)
- Seleção automática do adaptador LLM baseado no modelo escolhido
- Gerenciamento de sessões de chat
- Histórico de conversas
- Configurações personalizáveis
- Documentação completa com Swagger
- Tratamento global de exceções
- Logging estruturado
- Health Checks para API, banco de dados e serviços externos

## Health Checks

A aplicação implementa health checks abrangentes com os seguintes endpoints:

- `/health`: Status geral da aplicação e todas dependências
- `/health/ready`: Verificação de prontidão para tráfego
- `/health/live`: Verificação se a API está em execução
- `/health/database`: Status da conexão com o banco de dados
- `/health/external`: Status das conexões com serviços LLM externos

## Estrutura de Pastas

```
UserStoryIntegration/
├── src/
│   ├── UserStoryIntegration.API/                 # Projeto da API
│   ├── UserStoryIntegration.Application/         # Camada de aplicação
│   ├── UserStoryIntegration.Domain/              # Camada de domínio
│   ├── UserStoryIntegration.Infrastructure/      # Camada de infraestrutura
│   └── UserStoryIntegration.Shared/              # Recursos compartilhados
├── tests/                                  # Projetos de teste
├── docs/                                   # Documentação
└── scripts/                                # Scripts úteis
```

## Início Rápido

### Pré-requisitos

- .NET 9 SDK
- SQL Server (ou outro banco de dados compatível com EF Core)
- Chaves de API para os serviços LLM que você deseja usar (OpenAI, Anthropic, etc.)

### Instalação

1. Execute os scripts de criação do projeto para configurar a solução e todos os projetos

2. Restaure os pacotes NuGet
   ```
   dotnet restore
   ```

3. Configure suas chaves de API no arquivo `appsettings.json` ou use variáveis de ambiente

4. Aplique as migrações do banco de dados (após implementar o contexto do banco de dados)
   ```
   cd src/UserStoryIntegration.API
   dotnet ef database update
   ```

5. Execute a aplicação
   ```
   dotnet run
   ```

6. Acesse a documentação Swagger em `https://localhost:5001/`

## Configuração

As configurações principais estão no arquivo `appsettings.json`. Você pode sobrescrever qualquer configuração usando variáveis de ambiente ou arquivos de configuração específicos para ambiente (`appsettings.Development.json`, `appsettings.Production.json`).

### Chaves de API

Para usar os serviços LLM, você precisa configurar suas chaves de API:

```json
"LLMService": {
  "OpenAI": {
    "ApiKey": "seu-api-key-aqui"
  },
  "Anthropic": {
    "ApiKey": "seu-api-key-aqui"
  },
  "GoogleAI": {
    "ApiKey": "seu-api-key-aqui"
  }
}
```

## Licença

Este projeto está licenciado sob a licença MIT.
