# Gerador de Senhas Fortes

## Problema
- usuários não sabem criar senhas fortes e normalmente utilizam uma senha fraca para todas as contas

## Objetivo
- criar um gerador de senhas que permita salvar as senhas vinculando com uma lista de sites

## Usuários
- **Usuários Comuns:** Pessoas que utilizam a internet e precisam gerenciar credenciais de acesso para múltiplos sites com segurança.

## Histórias
- **US01 - Gerar Senha:** Como usuário comum, quero gerar uma senha forte customizável para que eu não precise inventar combinações vulneráveis.
- **US02 - Vincular a Site:** Como usuário comum, quero salvar a senha gerada atrelando-a a uma URL ou nome de site para organizá-la na minha lista.
- **US03 - Visualizar Lista:** Como usuário comum, quero ver a lista dos meus sites cadastrados e copiar as senhas de forma rápida.
- **US04 - Autenticação:** Como usuário comum, quero acessar minha conta com uma senha mestre para que meus acessos fiquem protegidos.

## Requisitos funcionais
- **RF01 - Customização da Senha:** O sistema deve permitir que o usuário defina o comprimento da senha (mínimo de 8 e máximo de 128 caracteres).
- **RF02 - Parâmetros de Complexidade:** O sistema deve oferecer opções para incluir/excluir letras maiúsculas, minúsculas, números e caracteres especiais.
- **RF03 - Cadastro de Credenciais:** O sistema deve registrar o nome do site, a URL (opcional) e a senha associada.
- **RF04 - Busca e Filtro:** O sistema deve permitir pesquisar os sites cadastrados pelo nome ou URL.

## Regras de negócio
- **RN01 - Criptografia em Repouso:** Nenhuma senha de site ou senha mestre pode ser armazenada em texto limpo no banco de dados. Devem ser usados algoritmos de hash/criptografia fortes (ex: BCrypt, PBKDF2 ou AES-256).
- **RN02 - Validação de Força Padrão:** Por padrão, as opções de caracteres especiais, números e maiúsculas devem vir ativadas com comprimento mínimo de 14 caracteres.

## Casos de borda
- **CB01 - Exclusão Absoluta de Caracteres:** Se o usuário desmarcar todas as opções de complexidade (letras, números, símbolos), o sistema deve impedir a geração.
- **CB02 - Sites Sem URL Válida:** O sistema deve aceitar termos simples no campo de site (ex: "Banco do Brasil") caso o usuário não saiba ou não queira inserir uma URL estruturada (http/https).

## Fora de escopo
- Importação ou exportação de senhas em lote através de arquivos CSV/JSON.

## Critérios de aceite
- O gerador deve criar uma combinação verdadeiramente aleatória baseada nos parâmetros escolhidos pelo usuário.
- O tempo de resposta ao listar ou pesquisar sites não deve passar de 1 segundo.
- A exclusão de uma conta de usuário deve apagar permanentemente todas as senhas associadas no banco de dados, sem chance de recuperação.