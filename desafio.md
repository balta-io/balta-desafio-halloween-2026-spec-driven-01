# Sobre o desafio

Este desafio consiste na criação de quatro arquivos principais para se trabalhar com Spec-Driven Developmento.

## Objetivo

- Realizar o setup inicial de um projeto com Spec-Driven
- Escrever sua primeira constituição
- Escrever sua primeira especificação
- Definir um plano para execução desta especificação
- Definir as tarefas deste plano
- Implementar estes itens com uso de IA

## Definição de pronto

Este desafio encontra-se concluído quando:

- A aplicação compila sem erros ou warnings
- Todos os testes passam
- A aplicação roda sem erros

## Sobre a API

A ideia deste desafio inicial é conseguir criar uma API simples, com dois endpoints, sendo um que gera senhas fortes seguindo um padrão e outro que dado um GUID, permite consultar uma senha associada.

As senhas tem que ter caracteres especiais, pelo menos 16 caracteres e não podem incluir espaços em branco.

Toda senha gerada é armazenada no banco e retorna um GUID para ser encontrada posteriormente.

Este é um App lúdico, apenas para aprendizado dos conceitos sobre SDD. Não há necessidade de encriptar senha ou outros recursos de segurança no momento. Não utilize este projeto em produção.

## Sugestões de Stack e Tecnologia

Você pode realizar este desafio com qualquer Stack que desejar, porém, recomendamos as seguintes:

### Stack

- .NET 10, C#, Minimal API
- Persistência em SQLite com EF Core
- Testes com xUnit

### Arquitetura

- Endpoint não acessa banco diretamente, sempre via serviço de aplicação
- Regra de negócio não vive no endpoint nem no handler HTTP
- Nenhuma dependência nova entra sem justificativa escrita no plano

### Testes e Qualidade

- Toda regra de negócio precisa ter teste unitário
- Todo endpoint precisa ter ao menos um teste de integração de caminho feliz
- Erro de validação sempre retorna 400 com ProblemDetails

# Antes de começar

Não se preocupe com a qualidade dos itens que você vai gerar aqui. Nos próximos vídeos e desafios vamos aprender a “forma correta” (E fácil) de gerar estes arquivos.

O intuito deste desafio é apenas te familiarizar com um projeto que faz uso de especificações como base.

# Setup

## Git e GitHub

### Passo 1: Acesse o GitHub

1. Abra o navegador e vá para [github.com](https://github.com/)
2. Clique no botão **"Sign up"** no canto superior direito

### Passo 2: Preencha as Informações

- **Email**: Use um email válido
- **Senha**: Crie uma senha forte com pelo menos 15 caracteres (ou 8 caracteres com números e símbolos)
- **Username**: Escolha um nome de usuário único para sua conta

### Passo 3: Verifique seu Email

1. GitHub enviará um email de confirmação
2. Clique no link de verificação
3. Complete o CAPTCHA se solicitado

### Passo 4: Personalize sua Conta

- Escolha seu plano (recomendado: **Free** para começar)
- Preencha informações de perfil opcionais
- Sua conta está pronta!

## 2. Instalando o Git

### Windows

1. Visite [git-scm.com](https://git-scm.com/)
2. Clique em **"Download for Windows"**
3. Execute o instalador e siga os passos padrão
4. Reinicie o computador após a instalação

### macOS

Abra o Terminal e execute:

```bash
brew install git
```

Se não tem Homebrew, instale primeiro em [brew.sh](https://brew.sh/)

### Linux (Ubuntu/Debian)

Abra o Terminal e execute:

```bash
sudo apt updatesudo apt install git
```

### Verificar Instalação

Abra o Terminal/Prompt de Comando e digite:

```bash
git --version
```

Você deve ver algo como: `git version 2.40.0`

### Configurar Git

Configure seu nome e email globalmente:

bash

```bash
git config --global user.name "Seu Nome"git config --global user.email "seu.email@example.com"
```

## 3. Fazendo Fork de um Repositório via Terminal

### O que é Fork?

Fork cria uma cópia completa de um repositório na sua conta GitHub. Você pode fazer alterações sem afetar o repositório original.

### Passo 1: Acesse o Repositório no GitHub

1. Visite o repositório público que deseja fazer fork
2. Clique no botão **"Fork"** no canto superior direito
3. GitHub criará uma cópia na sua conta

### Passo 2: Clone seu Fork Localmente

Abra o Terminal/Prompt de Comando e execute:

bash

```bash
git clone https://github.com/{SEU NOME DE USUARIO}/balta-desafio-halloween-2026-spec-driven-01.git
```

### Passo 3: Entre na Pasta do Repositório

```bash
cd balta-desafio-halloween-2026-spec-driven-01
```

## Visual Studio Code

Você não precisa de uma IDE ou Editor de Código, pode fazer tudo pelo bloco de notas ou pelo Visual Studio, Rider, ou mesmo qualquer IDE que já tenha.

Porém, pelo Visual Studio Code ter um ótimo suporte a Markdown e ser leve, nossa recomendação é a instalação e uso dele.

[Você pode fazer o download e instalação dele por aqui.](https://code.visualstudio.com/)

# Constituição

A constituição é o que não queremos repetir em todo prompt pelo resto da vida. São as regras do projeto que valem pra qualquer funcionalidade a ser adicionada.

Todos os itens da constituição precisam ser verificáveis, então atente-se ao que é tangível ou não durante sua descrição.

"Escrever código limpo" não é uma regra, é um desejo. "Nenhum acesso a dados dentro do endpoint" é uma regra, porque dá pra olhar o código e dizer se foi cumprida ou não.

Seu objetivo aqui é editar o arquivo [`constitution.md`](http://constitution.md) e adicionar as definições da constituição do projeto, seguindo os critérios que vimos no vídeo.

Abaixo está um template (apenas como sugestão) para implementação da sua constituição.

```markdown
# Constituição do projeto

## Stack
--

## Arquitetura
--

## Qualidade
--

## Convenções
--

## Governança
--
```

# Specification

Com o [`constitution.md`](http://constitution.md) criado, seu próximo passo é o [`spec.md`](http://spec.md) que é a parte de especificação do que deve ser feito.

Uma dica importante aqui é que não entra NADA de tecnologia no Spec, ou seja, nada de EF Core, Postgres, versão de .NET, Minimal API, nada.

Se você já escreve a stack aqui, você amarrou a solução antes de entender o problema. A especificação responde duas perguntas: o quê e por quê. O como fica pro plano, daqui a pouco.

Seu objetivo aqui é editar o arquivo [`spec.md`](http://constitution.md) e adicionar as definições da specificação do projeto, seguindo os critérios que vimos no vídeo.

Abaixo está um template (apenas como sugestão) para implementação da sua especificação.

```markdown
# Gerador de Senhas Fortes

## Problema
--

## Objetivo
--

## Usuários
--

## Histórias
--

## Requisitos funcionais
--

## Regras de negócio
--

## Casos de borda
--

## Fora de escopo
--

## Critérios de aceite
--
```

# Plan

Com o [`spec.md`](http://spec.md) pronto, vamos partir para o [`plan.md`](http://plan.md) que é a parte de planejamento de como vamos implementar as especificações.

Agora a gente pode falar de tecnologia com uma ressalva: Toda decisão vem com a justificativa do lado.
Não é "vamos usar Postgres". É "vamos usar SQLite porque o a regra XYZ exige que a senha seja persistida no banco".

Daqui a seis meses alguém vai perguntar por que a escolha foi essa, e a resposta vai estar escrita.

Seu objetivo aqui é editar o arquivo `plan.md` e adicionar as definições do planejamento técnico do projeto, seguindo os critérios que vimos no vídeo.

Abaixo está um template (apenas como sugestão) para implementação do seu plano.

```markdown
# Plano técnico

## Contexto
--

## Arquitetura
--

## Decisões
--

## Modelo de dados
--

## Contratos
--

## Riscos
--
```

# Tasks

Com o `plan.md` pronto, vamos partir para o `tasks.md` que é a parte de detalhar as tarefas que serão executadas durante o plano.

A regra da tarefa boa é essa: pequena, ordenada e verificável sozinha. E cada uma aponta pro requisito que a originou.

Por que tem que ser pequena? Porque agente com tarefa grande demais se perde. Ele começa bem, e lá pela metade esquece uma decisão que você tomou lá no começo.

Embora não exista uma “forma correta” de especificar estas tarefas, abaixo tem um exemplo de como elas podem ser feitas:

| ID | Tarefa | Origem | Depende de | Concluída quando |
| --- | --- | --- | --- | --- |
| T-01 | Criar projeto e estrutura de pastas | Constituição |  | Projeto compila |
| T-02 | Modelar a entidade Link e a migração | RF-01, RF-08 | T-01 | Migração aplica no banco |
| T-03 | Gerar código aleatório de 7 caracteres | RF-02, D-03 | T-01 | Teste unitário cobre formato e alfabeto |
| T-04 | Validar URL de destino | RN-02, borda 2048 | T-01 | Teste cobre http, https e inválidos |
| T-05 | Criar link com código automático | RF-01 | T-02, T-03, T-04 | Teste de integração cria e retorna 201 |
| T-06 | Aceitar código próprio com unicidade | RF-03, RN-01, D-04 | T-05 | Código duplicado retorna 409 |
| T-07 | Redirecionar por código | RF-04 | T-05 | Teste de integração retorna 302 |
| T-08 | Incrementar contador de forma atômica | RF-05, RN-04, D-02 | T-07 | Teste concorrente não perde contagem |
| T-09 | Endpoint de estatísticas com chave | RF-07, DC-03 | T-05 | Chave errada retorna 401 |
| T-10 | Cache em memória dos links mais acessados |  | T-07 | Latência de redirect cai |

Seu objetivo aqui é editar o arquivo `tasks.md` e adicionar as definições das tarefas do plano técnico projeto, seguindo os critérios que vimos no vídeo.

Abaixo está um template (apenas como sugestão) para implementação das tarefas como acima.

```markdown
# Tarefas

| ID | Tarefa | Origem | Depende de | Concluída quando |
|---|---|---|---|---|
| T-01 | Criar projeto e estrutura de pastas | Constituição | | Projeto compila |

```

# Implementação

Agora chegou a hora de gerar o sistema, utilizando o **prompt** descrito abaixo. No **Visual Studio Code** e na lateral direita, na coluna do **Copilot**, digite o seguinte prompt:

```markdown
Implemente APENAS a tarefa XXX do arquivo tasks.md.

Contexto obrigatório, siga à risca:
[cole constitution.md]
[cole a seção Decisões do plan.md]

Regras:
- Não implemente nenhuma outra tarefa
- Não altere arquivos fora do escopo da tarefa XXX
- Escreva o teste unitário junto, conforme a constituição
- Ao final, liste o que você mudou e qual requisito isso atende
```

Não esqueça de substituir as variáveis no prompt com seus dados reais.

# Submetendo o desafio

Para submeter o repositório, basta executar os comandos abaixo na ordem.

```bash
git add --all
git commit -m "Minha mensagem aqui"
git push -u origin main
```

## Conferindo a submissão

Abra a URL abaixo substituindo a variável pelo seu nome de usuário:

```markdown
https://github.com/{NOME DO SEU USUARIO}/balta-desafio-halloween-2026-spec-driven-01
```