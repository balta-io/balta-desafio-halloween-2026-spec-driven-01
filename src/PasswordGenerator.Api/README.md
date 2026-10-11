# Password Generator API

API (.NET 10, Minimal API, SQLite) com dois endpoints: gera uma senha forte, guarda-a e devolve um GUID; devolve a senha associada a um GUID.

> **Projeto educativo. Não use senhas reais.** As senhas ficam em texto claro na base de dados e o GUID é a única barreira de acesso.

## Como correr

```bash
dotnet run --project src/PasswordGenerator.Api
dotnet test
```

A base de dados `passwords.db` é criada e migrada no arranque (modo WAL). O ficheiro está no `.gitignore` (`*.db*`).

| Método | Rota                        | Resposta                                                        |
| ------ | --------------------------- | --------------------------------------------------------------- |
| POST   | `/api/v1/passwords`         | `201` `{ "id": "<guid>" }` e cabeçalho `Location`               |
| GET    | `/api/v1/passwords/{guid}`  | `200` `{ "senha": "<valor>" }`; `400` GUID inválido ou vazio; `404` inexistente |
| GET    | `/health`                   | `200` se a base de dados estiver acessível                      |
| GET    | `/openapi/v1.json`          | Documento OpenAPI (só em Development)                           |
| GET    | `/scalar`                   | Documentação interactiva Scalar (só em Development)             |

Erros usam `application/problem+json`, sem detalhes internos.

## Configuração

`appsettings.json`:

```json
{
  "ConnectionStrings": { "Default": "Data Source=passwords.db" },
  "PasswordGenerator": { "Length": 24, "ExcludeAmbiguous": false }
}
```

| Chave                                | Por omissão | Regra                                                         |
| ------------------------------------ | ----------- | ------------------------------------------------------------- |
| `PasswordGenerator:Length`           | `24`        | Entre 16 e 128. Fora do intervalo, a aplicação não arranca.   |
| `PasswordGenerator:ExcludeAmbiguous` | `false`     | Quando `true`, exclui `I`, `l`, `1`, `O` e `0`.               |
| `ConnectionStrings:Default`          | `Data Source=passwords.db` | Também pode vir de variável de ambiente (`ConnectionStrings__Default`). |

## Riscos aceites

- **R1, GUID como única barreira.** Não há autenticação nem limite de taxa. Quem obtiver o GUID (histórico, logs de proxy, _referrer_) lê a senha. Mitigações: GUID v4 aleatório, logs pelo _route template_ e `Cache-Control: no-store`. A autenticação é a primeira evolução recomendada.
- **R2, senhas em texto claro.** A cifragem está fora de escopo da constituição. Quem aceder ao ficheiro `.db` lê todas as senhas. Mitigações: `*.db*` no `.gitignore` e permissões do ficheiro. Reintroduzir cifra exige nova spec e emenda à constituição.

**Não use senhas reais.** Os dois riscos acima são aceites apenas porque este é um projeto educativo.
