# Plano técnico

## Contexto
- Desenvolvimento de uma API REST segura e performática para um Gerador de Senhas Fortes.
- O sistema permite a geração de credenciais aleatórias complexas e o armazenamento seguro dessas senhas associadas a sites/serviços específicos para um Usuário Comum.
- O armazenamento será local e leve, utilizando SQLite, garantindo isolamento total dos dados de cada instância da API.

## Arquitetura
- **Camada de Endpoints (Web API):** Responsável apenas pelo recebimento das requisições HTTP, validação básica dos modelos de entrada e retorno de respostas. Não possui lógica de negócio ou acesso ao banco. Em caso de erro de validação, formata a resposta estritamente via `ProblemDetails` (HTTP 400).
- **Camada de Aplicação / Handlers:** Contém os serviços de aplicação que orquestram o fluxo de dados. Os endpoints invocam estes serviços, que se comunicam com o banco de dados e aplicam as regras de criptografia e negócio.
- **Camada de Domínio / Serviços Core:** Centraliza o algoritmo de geração de senhas criptograficamente seguras e as regras puras de validação.
- **Camada de Infraestrutura (EF Core + SQLite):** Gerencia a persistência dos dados de forma relacional utilizando criptografia para proteger os dados sensíveis em repouso.

## Decisões
- **DP01 - Provedor Criptográfico do .NET:** Para a geração das senhas aleatórias (US01/RF01), utilizaremos a classe `System.Security.Cryptography.RandomNumberGenerator` em vez do `System.Random`, garantindo entropia segura contra ataques de predição.
- **DP02 - Estratégia de Criptografia (AES-256 + PBKDF2):** 
  - A *Senha Mestre* do usuário passará por um hash usando **PBKDF2** (com salt e alto número de iterações) para autenticação.
  - As *Senhas dos Sites* serão criptografadas via **AES-256** utilizando uma chave derivada da senha mestre do usuário. O vetor de inicialização (IV) será armazenado junto ao registro. Dessa forma, se o arquivo SQLite for exposto, as senhas continuam ilegíveis.
- **DP03 - Isolamento por Middleware:** Um middleware global capturará exceções inesperadas para formatá-las em `ProblemDetails` (HTTP 500), limpando stack traces e mensagens internas de infraestrutura do SQLite antes de responder ao cliente.
- **DP04 - Restrição de Dependências Nuget:** Conforme a constituição do projeto, nenhuma biblioteca externa de criptografia ou validação (como FluentValidation) será adicionada sem uma justificativa prévia por escrito. Utilizaremos os recursos nativos do .NET 10 (`DataAnnotations` e `System.Security.Cryptography`).
- **DP05 - Dependências de Teste:** O projeto de testes utilizará `xunit`, `Microsoft.NET.Test.Sdk` e `xunit.runner.visualstudio` para atender à stack definida na constituição e permitir descoberta e execução padrão dos testes com `dotnet test`. Nenhuma dependência adicional de teste será incluída nesta etapa.

## Modelo de dados

### Tabela: `users`

| Campo | Tipo SQLite | Restrições | Descrição |
| :--- | :--- | :--- | :--- |
| `id` | TEXT / GUID | PRIMARY KEY | Identificador único do usuário. |
| `email` | TEXT | UNIQUE, NOT NULL | E-mail do usuário (usado para login). |
| `master_password_hash` | TEXT | NOT NULL | Hash da senha mestre gerado via PBKDF2. |
| `created_at` | TEXT | NOT NULL | Data de criação da conta. |

### Tabela: `credentials`

| Campo | Tipo SQLite | Restrições | Descrição |
| :--- | :--- | :--- | :--- |
| `id` | TEXT / GUID | PRIMARY KEY | Identificador único da credencial. |
| `user_id` | TEXT / GUID | FOREIGN KEY -> `users(id)` ON DELETE CASCADE | Vínculo com o usuário dono da senha. |
| `site_name` | TEXT | NOT NULL | Nome do site/serviço (ex: "Banco do Brasil"). |
| `site_url` | TEXT | NULL | URL do site (opcional). |
| `username` | TEXT | NOT NULL | E-mail ou usuário de acesso naquele site. |
| `encrypted_password` | TEXT | NOT NULL | Senha do site criptografada em AES-256 (Base64). |
| `iv` | TEXT | NOT NULL | Vetor de Inicialização do AES usado nesta senha (Base64). |
| `updated_at` | TEXT | NOT NULL | Data da última alteração. |

*Nota: Será criado um índice único composto por `(user_id, site_name, username)` para garantir a regra **RN03**.*

## Contratos

### 1. Gerar Senha (GET `/api/v1/passwords/generate`)
* **Query Parameters:** `length` (int, default 14), `includeUpper` (bool, default true), `includeLower` (bool, default true), `includeNumbers` (bool, default true), `includeSpecial` (bool, default true).
* **Resposta Sucesso (200 OK):**
```json
{
  "password": "gX9!mQ2@zP4#kL1$"
}
```
* **Resposta Erro (400 Bad Request - Caso de Borda CB01/CB02):**
```json
{
  "type": "https://ietf.org",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Complexity": ["At least one character type type must be selected."]
  }
}
```

### 2. Salvar Credencial (POST `/api/v1/credentials`)
* **Headers:** `Authorization: Bearer <token>`
* **Request Body:**
```json
{
  "siteName": "GitHub",
  "siteUrl": "https://github.com",
  "username": "dev_user",
  "password": "senha_gerada_em_texto_limpo"
}
```
* **Resposta Sucesso (201 Created):** Retorna o objeto criado (sem expor o IV ou a senha criptografada aberta).

### 3. Listar/Filtrar Credenciais (GET `/api/v1/credentials`)
* **Query Parameters:** `search` (string, opcional - busca por nome ou URL)
* **Resposta Sucesso (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "siteName": "GitHub",
    "siteUrl": "https://github.com",
    "username": "dev_user"
  }
}
```

### 4. Obter Senha Descriptografada (GET `/api/v1/credentials/{id}/password`)
* **Resposta Sucesso (200 OK):** `{"password": "senha_original"}` (Após descriptografia em tempo de execução usando a chave derivada).

## Riscos
- **R01 - Perda da Senha Mestre (Risco Crítico):** Como o sistema adota a premissa de segurança de que o servidor não conhece a senha limpa, se o usuário esquecer a senha mestre, o dado será permanentemente irrecuperável. 
  - *Mitigação:* Exibir avisos claros na interface consumidora durante a criação da conta.
- **R02 - Concorrência no SQLite (Risco Médio):** Sendo um banco de dados baseado em arquivo único, escritas simultâneas massivas podem bloquear o arquivo (`database is locked`).
  - *Mitigação:* Configurar o modo de paginação do SQLite para `WAL` (Write-Ahead Logging) no EF Core e manter o escopo das transações o mais curto possível.
- **R03 - Engenharia reversa de memória (Risco Baixo):** Senhas trafegando ou residindo em strings tradicionais na memória da API podem ficar vulneráveis a dumps de memória.
  - *Mitigação:* Processar as senhas limpas rapidamente e avaliar o uso de estruturas como `ReadOnlySpan<char>` para manipulação no algoritmo do gerador.
