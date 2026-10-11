# Constituição do projeto

## Stack

- Backend: .NET 10 (ASP.NET Core Web API).
- Linguagem: C# 14.
- API: ASP.NET Core Minimal API.
- Base de dados: SQLite
- ORM: Entity Framework Core com o provider SQLite.
- Testes: xUnit, NSubstitute.
- Documentação: OpenAPI integrado ao ASP.NET Core.
- Validação: validação nativa de Minimal APIs e/ou FluentValidation.
- Observabilidade: logs estruturados com Serilog; métricas e health checks.

## Arquitetura

- Arquitetura: Vertical Slice Architecture (VSA).
- Cada slice representa um caso de uso completo e autocontido.
- Um slice pode conter: endpoint Minimal API, request/response, handler, validação, regras de domínio, mapeamentos e testes.
- Slices não devem conhecer detalhes internos uns dos outros.
- Código partilhado apenas quando for genuinamente comum: abstrações, opções de configuração, contratos transversais e utilitários seguros.

## Qualidade

- Cada slice novo entra com testes unitários e, quando aplicável, testes de integração.
- Todo endpoint precisa de pelo menos um teste de integração do caminho feliz.
- Testes unitários obrigatórios:
  - senha tem pelo menos 16 caracteres;
  - senha contém pelo menos um caractere especial;
  - senha não contém espaços em branco;
  - senha usa apenas caracteres permitidos;
  - comprimento inferior a 16 é rejeitado;
  - comprimento superior a 128 é rejeitado;
  - geração produz senhas diferentes em chamadas sucessivas.
- Testes de integração obrigatórios:
  - `POST /api/v1/passwords` gera senha, persiste no SQLite e devolve GUID;
  - `GET /api/v1/passwords/{guid}` devolve a senha associada ao GUID;
  - GUID inexistente devolve `404`;
  - pedido inválido devolve `400` com `ProblemDetails`.
- O build e todos os testes devem passar antes do merge.

## Convenções

- Um slice por caso de uso; diretórios em PascalCase com nomes de ação.
- Diretórios de slices em PascalCase: `GeneratePassword`, `GetPasswordByGuid`.
- Usar `TypedResults` nos endpoints Minimal API.
- Requests e responses são `record` imutáveis.
- Commits seguem Conventional Commits: `feat:`, `fix:`, `test:`, `docs:`, `refactor:`, `chore:`.
- Branches: `feature/nome-curto`, `fix/nome-curto`, `chore/nome-curto`.
- Configurações sensíveis ficam em variáveis de ambiente, user secrets ou secret manager; nunca no repositório.
- A connection string do SQLite pode ficar em `appsettings.json`, dado o caráter educativo do projeto.
- Dependências novas exigem justificativa, análise de manutenção, licença e vulnerabilidades.

## Governança

- Alterações nesta constituição exigem Pull Request dedicado e aprovação do responsável técnico.
- Cada especificação deve ser verificada contra esta constituição antes da implementação
- Novas dependências exigem justificativa escrita no plano.
- Novas funcionalidades exigem nova especificação e atualização do escopo.
- Recursos como autenticação, autorização, cifragem, rate limiting, auditoria e observabilidade avançada estão fora do escopo atual.
