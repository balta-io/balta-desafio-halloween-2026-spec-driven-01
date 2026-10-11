# Plano técnico

## Contexto

API com dois endpoints: `POST /api/v1/passwords` gera uma senha forte, grava-a e devolve um GUID; `GET /api/v1/passwords/{guid}` devolve a senha associada. Está concluído quando RF01–RF09 estiverem implementados e os critérios de aceite aplicáveis (CA01–CA11) passarem.

**Fora de escopo (spec secção 8 + constituição):** autenticação, autorização, **cifragem**, limite de taxa, auditoria, observabilidade avançada, alterar/eliminar/expirar/listar senhas, parâmetros do cliente, UI, multi-tenant, versionamento e deployment.

### Verificação contra a constituição

A constituição prevalece sobre a spec. Pontos verificados:

| #   | Ponto                                                                                                                                                      | Resolução                                                                                                                                                           |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | **Cifragem** fora de escopo. A spec já não tem regra de cifra, mas ficam resíduos (ver tabela seguinte).                                                   | **Sem cifragem:** a senha fica em texto claro no SQLite (D07, risco R2).                                                                                            |
| 2   | **Base de dados:** SQLite.                                                                                                                                 | EF Core + SQLite, também nos testes de integração.                                                                                                                  |
| 3   | **Comprimento.** A constituição exige testes "comprimento < 16 / > 128 é rejeitado", mas o cliente não envia comprimento.                                  | A rejeição é regra de **domínio** (`PasswordPolicy`) e de **configuração** (falha no arranque). Não há `400` por comprimento em HTTP.                               |
| 4   | **Nomes dos slices:** `GeneratePassword` e `GetPasswordByGuid`.                                                                                            | Adoptados.                                                                                                                                                          |
| 5   | **Endpoints finos:** injectam e chamam o **serviço de aplicação** do slice. Nunca injectam `DbContext` nem contêm geração, validação ou consulta de senha. | Cada slice tem o seu serviço (`IGeneratePasswordService`, `IGetPasswordByGuidService`). O endpoint só traduz HTTP. Há um teste de arquitectura que o garante (D02). |

## Arquitetura

**Stack (constituição):** .NET 10, C# 14, ASP.NET Core Minimal API, EF Core + SQLite, xUnit + NSubstitute, OpenAPI integrado, Serilog, métricas e _health checks_.

**Estilo:** Vertical Slice Architecture. Cada slice é um caso de uso autocontido: endpoint, serviço de aplicação, request/response, regras de domínio e testes. **Os slices não se conhecem.** Só é partilhado o que é genuinamente comum: persistência, tratamento de erros e observabilidade.

```
Cliente
  │ POST /api/v1/passwords            GET /api/v1/passwords/{guid}
  ▼                                      ▼
┌─ Slice GeneratePassword ──────┐   ┌─ Slice GetPasswordByGuid ─────┐
│ Endpoint  (só HTTP)           │   │ Endpoint  (só HTTP)           │
│    ▼                          │   │    ▼                          │
│ IGeneratePasswordService      │   │ IGetPasswordByGuidService     │
│    ├ IPasswordGenerator       │   │    └ Guid.TryParse + consulta │
│    │   + PasswordPolicy       │   │                               │
│    └ IGuidGenerator           │   │                               │
└──────────────┬────────────────┘   └───────────────┬───────────────┘
               └───────► Shared/Persistence ◄───────┘
                      AppDbContext · StoredPassword
                                │
                             SQLite
```

**Estrutura de pastas**

```
src/PasswordGenerator.Api/
  Program.cs
  Features/
    GeneratePassword/
      GeneratePasswordEndpoint.cs        (MapGeneratePassword; só HTTP)
      IGeneratePasswordService.cs
      GeneratePasswordService.cs         (orquestra geração, validação e persistência)
      GeneratePasswordResponse.cs        (record)
      PasswordPolicy.cs                  (limites, conjuntos, validação)
      PasswordGenerator.cs               (IPasswordGenerator)
      PasswordGeneratorOptions.cs
      IGuidGenerator.cs
      GeneratePasswordSetup.cs           (AddGeneratePassword: DI do slice)
    GetPasswordByGuid/
      GetPasswordByGuidEndpoint.cs       (MapGetPasswordByGuid; só HTTP)
      IGetPasswordByGuidService.cs
      GetPasswordByGuidService.cs
      GetPasswordByGuidResult.cs         (Found / InvalidGuid / NotFound)
      GetPasswordByGuidResponse.cs       (record)
      GetPasswordByGuidSetup.cs
  Shared/
    Persistence/AppDbContext.cs, StoredPassword.cs, Migrations/
    Errors/ProblemDetailsSetup.cs
    Observability/DatabaseHealthCheck.cs, PasswordMetrics.cs
tests/PasswordGenerator.Tests/
  Features/GeneratePassword/
  Features/GetPasswordByGuid/
  Architecture/EndpointsDoNotUseDbContextTests.cs
  Shared/ApiFactory.cs
```

**Fluxo do POST** (`Results<Created<GeneratePasswordResponse>, ProblemHttpResult>`)

1. O **endpoint** chama `IGeneratePasswordService.GenerateAsync` e devolve `TypedResults.Created(...)` com `Location`.
2. O **serviço** obtém o `Guid` de `IGuidGenerator` e a senha de `IPasswordGenerator`, com a `PasswordPolicy` das opções.
3. `PasswordPolicy.Validate` confirma a composição (RN08, asserção defensiva).
4. O serviço adiciona o `StoredPassword` ao `AppDbContext` e chama `SaveChangesAsync` (uma única operação, logo sem registo parcial, RN12).
5. Devolve o `GeneratePasswordResponse` com `{ "id": "<guid>" }`.

Se o SQLite devolver violação de chave primária (código estendido 1555), o serviço repete os passos 2–4 até 3 vezes. Qualquer outra falha propaga-se para o handler global e resulta em `500`.

**Fluxo do GET** (`Results<Ok<GetPasswordByGuidResponse>, ValidationProblem, ProblemHttpResult>`)

1. O **endpoint** passa o texto recebido a `IGetPasswordByGuidService` e traduz o resultado em `TypedResults`. É também onde se define `Cache-Control: no-store`.
2. O **serviço** faz `Guid.TryParse` e rejeita `Guid.Empty`: `InvalidGuid`, sem tocar na base de dados, que o endpoint converte em `400` via `TypedResults.ValidationProblem`.
3. O serviço consulta por chave primária (`AsNoTracking`). Se não existir: `NotFound`, convertido em `404` via `TypedResults.Problem`.
4. Se existir: `Found(senha)`, convertido em `200` com `{ "senha": "<valor>" }`.

**Observabilidade**

- **Serilog** com logs estruturados e _request logging_ pelo _route template_, nunca pelo caminho bruto nem com corpos.
- **Health check** em `/health`, com verificação própria (`Database.CanConnectAsync`).
- **Métricas** com `System.Diagnostics.Metrics` (`Meter` com contadores de senhas geradas e consultadas). O exportador fica fora de escopo (observabilidade avançada).
- **OpenAPI** integrado (`AddOpenApi`/`MapOpenApi`), exposto em Development.

**Estratégia de testes** (xUnit + NSubstitute; build e testes verdes antes do merge)

| Tipo                     | Teste                                                                                                                                                                                                                                                                                                          |
| ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Unitário (obrigatório)   | Senha ≥ 16 caracteres; ≥ 1 especial; sem espaços em branco; só caracteres permitidos; comprimento < 16 rejeitado; comprimento > 128 rejeitado; chamadas sucessivas produzem senhas diferentes.                                                                                                                 |
| Unitário (extra)         | 1000 senhas sem repetições (CA06); `ExcludeAmbiguous` activo; `GeneratePasswordService` com `IGuidGenerator` substituído por NSubstitute (colisão com retry); `GetPasswordByGuidService` com GUID em maiúsculas, sem hífenes, vazio e texto livre. Os serviços usam um `AppDbContext` sobre SQLite em memória. |
| Integração (obrigatório) | `POST` gera, persiste no SQLite e devolve GUID (CA01, CA02); `GET` devolve a senha do GUID (CA07, CA08); GUID inexistente: `404` (CA09); GUID inválido: `400` com `ProblemDetails` (CA10).                                                                                                                     |
| Integração (extra)       | Falha simulada na gravação: `500`, sem registo (CA11), via `SaveChangesInterceptor` que lança excepção no `ApiFactory`; método não suportado: `405`; `/health`; 50 `POST` em paralelo.                                                                                                                         |
| Arquitectura             | Por reflexão, nenhum método de endpoint tem parâmetro `AppDbContext` (D02). Sem dependência nova.                                                                                                                                                                                                              |

O `ApiFactory` usa `WebApplicationFactory` com uma `SqliteConnection` em memória mantida aberta durante o teste.

## Decisões

| ID  | Decisão                                                                                                                                                            | Justificação / alternativa rejeitada                                                                                                                                                                                                    |
| --- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| D01 | Dois slices, `GeneratePassword` e `GetPasswordByGuid`, sem dependências entre si. Cada um expõe `Add…` (DI) e `Map…` (rota).                                       | Constituição. A persistência partilhada fica em `Shared/Persistence`.                                                                                                                                                                   |
| D02 | **Endpoints finos:** só injectam e chamam o serviço de aplicação do slice. O serviço usa o `AppDbContext` directamente. Sem repositório nem MediatR.               | Constituição (nunca `DbContext` no endpoint; nunca geração, validação ou consulta no endpoint). O `DbContext` já é _unit of work_, e um repositório seria cerimónia para dois casos de uso. Um teste de arquitectura impede regressões. |
| D03 | O `GET` recebe `{guid}` como `string`; o **serviço** faz `Guid.TryParse` e rejeita `Guid.Empty`, devolvendo um resultado tipado.                                   | Com `{guid:guid}` no routing, um GUID inválido daria `404`, não `400` (RF05, CA10). Manter o parse no serviço deixa o endpoint só com mapeamento HTTP.                                                                                  |
| D04 | Geração: 1 carácter garantido de cada classe, restantes do alfabeto completo, depois Fisher-Yates, tudo com `RandomNumberGenerator.GetInt32`                       | `GetInt32` é uniforme (sem viés de módulo). Garante RN03/RN14 por construção. Nunca `System.Random` (RN06).                                                                                                                             |
| D05 | Comprimento fixo de **24**, configurável entre 16 e 128 e validado no arranque (`ValidateOnStart`)                                                                 | Cerca de 150 bits de entropia com 88 símbolos. A `PasswordPolicy` rejeita também comprimentos inválidos (testes obrigatórios).                                                                                                          |
| D06 | `ExcludeAmbiguous` como opção de configuração, por omissão `false`                                                                                                 | RN07 sem violar a secção 8.                                                                                                                                                                                                             |
| D07 | **Sem cifragem**: coluna `Senha` em texto claro                                                                                                                    | Constituição. Reintroduzir cifra exige nova spec e emenda à constituição (Governança).                                                                                                                                                  |
| D08 | `Guid.NewGuid()` (v4, aleatório), atrás de `IGuidGenerator`                                                                                                        | O GUID é a única barreira de acesso: **não usar v7**, que é parcialmente previsível. A interface permite testar colisões com NSubstitute.                                                                                               |
| D09 | Validação nativa, sem FluentValidation                                                                                                                             | O único input externo é um GUID. Evita dependência nova.                                                                                                                                                                                |
| D10 | `AddProblemDetails` + `UseExceptionHandler`                                                                                                                        | RF09: formato único, sem _stack traces_. A mensagem do `404` não revela GUIDs parecidos.                                                                                                                                                |
| D11 | Senhas nunca vão para logs: Serilog sem corpos, com _route template_                                                                                               | Já não é regra da spec, mas é prática mínima, dado que o GUID também é segredo de facto.                                                                                                                                                |
| D12 | SQLite: connection string em `appsettings.json` (permitido), WAL activado no arranque, migrações EF aplicadas com `Migrate()` no arranque, `*.db*` no `.gitignore` | Constituição + caráter educativo. O `.gitignore` evita enviar para o repositório o ficheiro com senhas em claro.                                                                                                                        |
| D13 | Records imutáveis para request/response; `TypedResults` em todos os endpoints                                                                                      | Constituição.                                                                                                                                                                                                                           |
| D14 | Git: branch `feature/password-api`, Conventional Commits (`feat:`, `test:`, `docs:`, …)                                                                            | Constituição.                                                                                                                                                                                                                           |

### Dependências (justificação, licença, manutenção)

Todas já decorrem da constituição. Não se adicionam outras.

| Pacote                                                       | Justificação                                     | Licença          | Manutenção               |
| ------------------------------------------------------------ | ------------------------------------------------ | ---------------- | ------------------------ |
| `Microsoft.EntityFrameworkCore.Sqlite` (+ `.Design`, só dev) | ORM e provider (constituição)                    | MIT              | Microsoft                |
| `Serilog.AspNetCore`                                         | Logs estruturados (constituição)                 | Apache-2.0       | Projecto Serilog, activo |
| `Microsoft.AspNetCore.OpenApi`                               | OpenAPI integrado (constituição)                 | MIT              | Microsoft                |
| `xunit`, `Microsoft.NET.Test.Sdk`                            | Testes (constituição)                            | Apache-2.0 / MIT | xUnit.net / Microsoft    |
| `NSubstitute`                                                | _Mocks_ (constituição)                           | BSD-3-Clause     | Activo                   |
| `Microsoft.AspNetCore.Mvc.Testing`                           | `WebApplicationFactory` nos testes de integração | MIT              | Microsoft                |

**Não adoptados:** FluentValidation, MediatR, exportadores OpenTelemetry, pacote de health checks para EF.

**Vulnerabilidades:** verificar com `dotnet list package --vulnerable --include-transitive` antes do merge.

## Modelo de dados

**Tabela `Passwords`** (entidade `StoredPassword`)

| Coluna         | Tipo SQLite | Restrições                                           | Notas                                                                  |
| -------------- | ----------- | ---------------------------------------------------- | ---------------------------------------------------------------------- |
| `Id`           | `TEXT`      | PK, não nulo                                         | GUID gerado pelo servidor (RN10), independente da senha.               |
| `Senha`        | `TEXT`      | não nulo, `CHECK (length(Senha) BETWEEN 16 AND 128)` | **Texto claro** (D07). O `CHECK` é defesa em profundidade (RN01/RN02). |
| `CreatedAtUtc` | `TEXT`      | não nulo                                             | Informativo, via `TimeProvider`.                                       |

- **Imutabilidade (RN13):** entidade sem _setters_ públicos e sem caminho de código para `UPDATE` ou `DELETE`.
- **Sem índices adicionais:** todo o acesso é por chave primária.
- **Migrações:** EF Core, aplicadas no arranque.

**Configuração**

```json
{
  "ConnectionStrings": { "Default": "Data Source=passwords.db" },
  "PasswordGenerator": { "Length": 24, "ExcludeAmbiguous": false }
}
```

## Contratos

### `POST /api/v1/passwords`

- **Pedido:** sem corpo (ignorado se existir).
- **`201 Created`**, cabeçalho `Location: /api/v1/passwords/{id}`:
  ```json
  { "id": "3f2504e0-4f89-11d3-9a0c-0305e82c3301" }
  ```
- **`500`** em falha de persistência (`ProblemDetails`), sem GUID devolvido.

### `GET /api/v1/passwords/{guid}`

- **`200 OK`**, cabeçalho `Cache-Control: no-store`:
  ```json
  { "senha": "k#9Tq.vB2m-Xw!4Lr_Hs8Zp" }
  ```
- **`400`** GUID mal formado ou `00000000-0000-0000-0000-000000000000` (`HttpValidationProblemDetails`).
- **`404`** GUID válido sem registo (`ProblemDetails`).
- **`500`** erro interno.

### Erros (RF09)

`Content-Type: application/problem+json`, com `detail` genérico. Nunca inclui o GUID pedido, mensagens de excepção ou nomes de tabelas.

| Situação                         | Estado        |
| -------------------------------- | ------------- |
| `GET` com GUID inválido ou vazio | 400           |
| `GET` com GUID inexistente       | 404           |
| Método não suportado             | 405 (routing) |
| Erro interno                     | 500           |

### Contratos internos dos serviços

```csharp
public interface IGeneratePasswordService
{
    Task<GeneratePasswordResponse> GenerateAsync(CancellationToken ct);
}

public interface IGetPasswordByGuidService
{
    Task<GetPasswordByGuidResult> GetAsync(string id, CancellationToken ct);
}
```

`GetPasswordByGuidResult` tem três variantes: `Found(string Senha)`, `InvalidGuid` e `NotFound`.

### Endpoints de infra (fora do contrato de negócio)

`GET /health` e `GET /openapi/v1.json` (só em Development).

**Nota para os testes:** o `System.Text.Json` escapa `<`, `>`, `&` e `+` como `\uXXXX`. É JSON válido e desserializa para o mesmo valor. Os testes (CA07 e secção 7) devem comparar **após desserializar**, não os bytes do corpo.

## Riscos

| ID  | Risco                                                                                                                                                            | Impacto                     | Mitigação                                                                                                                                                                       |
| --- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| R1  | **Sem autenticação, o GUID é a única barreira.** Quem o obtiver (histórico, logs de proxy, _referrer_) lê a senha. Assumido na secção 8, mas é um risco central. | Alto                        | D08 (v4 aleatório), logs pelo _route template_, `no-store`. Registar a aceitação no README. Autenticação como primeira evolução.                                                |
| R2  | **Senhas em texto claro no SQLite.** A cifragem está fora de escopo na constituição. Quem aceder ao ficheiro `.db` lê todas as senhas.                           | Alto                        | Aceite por ser projecto educativo. `*.db*` no `.gitignore`, permissões do ficheiro, nunca usar senhas reais. Reintroduzir cifra exige nova spec e emenda à constituição por PR. |
| R3  | **Resíduos da cifra na spec:** CA12 (refere H5 e uma "secção 6" inexistentes) e o ponto da secção 8 sobre cifra em repouso.                                      | Médio                       | Remover ambos antes de implementar (Governança: verificação da spec contra a constituição).                                                                                     |
| R4  | **Concorrência no SQLite:** um único escritor; sob `POST` simultâneos pode ocorrer _database is locked_.                                                         | Médio                       | WAL e timeout de comando. Teste de integração com 50 `POST` em paralelo (secção 7).                                                                                             |
| R5  | **Senha em logs** por _request logging_, tratamento de excepções ou métricas. A regra saiu da spec.                                                              | Alto                        | D11. Teste que confirma que nenhuma saída de log contém a senha gerada.                                                                                                         |
| R6  | **Detecção de colisão específica do SQLite** (código 1555) confundida com outros erros.                                                                          | Baixo                       | Isolar a detecção no `GeneratePasswordService`. Testar com `IGuidGenerator` substituído por NSubstitute e GUID pré-existente.                                                   |
| R7  | **Enumeração de GUIDs** sem limite de taxa.                                                                                                                      | Baixo (122 bits aleatórios) | Aceite (secção 8). Só é viável se o GUID vazar (R1).                                                                                                                            |
| R8  | **Fuga do ficheiro `.db`** para o repositório ou _backups_.                                                                                                      | Médio                       | `.gitignore` (D12) e revisão no PR.                                                                                                                                             |
| R9  | **Regressão da regra dos endpoints finos** (alguém injecta `AppDbContext` num endpoint).                                                                         | Baixo                       | Teste de arquitectura por reflexão, a correr em CI.                                                                                                                             |
