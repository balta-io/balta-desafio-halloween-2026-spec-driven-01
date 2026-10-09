# Constituição do projeto

## Stack
- .NET Core 10 API, C#
- Persistência em SQLite com EF Core
- Testes com xUnit

## Arquitetura
- Endpoint não acessa banco diretamente, sempre via serviço de aplicação
- Regra de negócio não vive no endpoint nem no handler HTTP
- Nenhuma dependência nova entra sem justificativa escrita no plano

## Qualidade
- Toda regra de negócio precisa ter teste unitário
- Todo endpoint precisa ter ao menos um teste de integração de caminho feliz
- Erro de validação sempre retorna 400 com ProblemDetails

## Convenções
- Código todo escrito em inglês
- Commits estruturados obrigatoriamente seguindo o padrão Conventional Commits (ex: feat:, fix:, chore:)

## Governança
--