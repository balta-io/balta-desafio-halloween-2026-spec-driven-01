# Tarefas

| ID | Tarefa | Origem | Depende de | Concluída quando |
|---|---|---|---|---|
| T-01 | Criar projeto e estrutura de pastas | Constituição | | Projeto .NET Core 10 Web API compila com sucesso. |
| T-02 | Configurar infraestrutura do SQLite e EF Core | Constituição / Plan | T-01 | DbContext criado, string de conexão ajustada e modo WAL ativado. |
| T-03 | Implementar as Entidades e Migrations | Plan (Modelo de Dados) | T-02 | Migrations criadas e aplicadas gerando as tabelas `users` e `credentials`. |
| T-04 | Criar Middleware de captura global de erros | Constituição / Plan | T-01 | Exceções não tratadas retornam formato `ProblemDetails` com HTTP 500. |
| T-05 | Desenvolver o Serviço de Geração de Senhas | Spec (US01) / Plan | T-01 | Algoritmo implementado nativamente usando `RandomNumberGenerator`. |
| T-06 | Criar Endpoint de Geração de Senhas | Spec (RF01/RF02) / Plan | T-05 | Rota `GET /api/v1/passwords/generate` responde com a senha ou HTTP 400. |
| T-07 | Desenvolver os Serviços de Criptografia (AES-256) | Plan (Decisões) | T-01 | Funções de cifrar e decifrar dados prontas e testadas isoladamente. |
| T-08 | Implementar Autenticação e Cadastro de Usuário | Spec (US04) / Plan | T-03 | Rota de registro/login valida credenciais usando PBKDF2 com Salt. |
| T-09 | Implementar Endpoint de Salvar Credencial | Spec (US02) / Plan | T-07 / T-08 | Rota `POST /api/v1/credentials` valida duplicidade (RN03) e salva o registro criptografado. |
| T-10 | Implementar Endpoint de Listagem e Filtro | Spec (US03) / Plan | T-08 | Rota `GET /api/v1/credentials` retorna dados básicos filtrados sem expor senhas limpas. |
| T-11 | Implementar Endpoint de Revelar Senha | Spec (US03) / Plan | T-07 / T-09 | Rota `GET /api/v1/credentials/{id}/password` descriptografa e exibe a senha original. |
| T-12 | Implementar Exclusão de Conta em Cascata | Spec (Critérios de Aceite) | T-08 | Rota de exclusão apaga o usuário e limpa todos os seus registros atrelados no SQLite. |
| T-13 | Escrever os Testes Unitários de Regra de Negócio | Constituição | T-05 / T-07 | Suíte de testes do xUnit cobre todas as RNs e algoritmos com 100% de sucesso. |
| T-14 | Escrever os Testes de Integração (Caminho Feliz) | Constituição | T-06 / T-09 / T-10 | Testes automatizados executam chamadas HTTP simuladas contra os endpoints com sucesso. |
