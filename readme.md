<img width="100%" alt="Halloween 2026" src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/banner.jpg" />

## 🎃 Halloween - Desafio 1

Oi, eu sou o Raul Mateia da Silva e este é o espaço onde compartilho minha jornada de aprendizado durante o desafio **Halloween 2026**, realizado pelo [balta.io](https://balta.io). 👻

Aqui você vai encontrar projetos, exercícios e códigos que estou desenvolvendo durante o desafio.

### Sobre este desafio

Neste desafio o objetivo é consolidar os fundamentos do Spec Driven Development, criando uma constituição, especificação, planejamento e tarefas manualmente para serem implementadas pela IA posteriormente.

#### O que foi implementado

Arquitectura (VSA)

- Features/GeneratePassword: endpoint fino, GeneratePasswordService, PasswordGenerator, PasswordPolicy, GuidGenerator, opções e DI.
- Features/GetPasswordByGuid: endpoint fino, GetPasswordByGuidService, resultado tipado (Found, InvalidGuid, NotFound).
- Shared/Persistence: AppDbContext, StoredPassword imutável, migração InitialCreate.
- Shared/Errors e Shared/Observability: ProblemDetails, Serilog, métricas e health check.

Regras e decisões

- Geração: 1 carácter de cada classe, resto do alfabeto completo, Fisher-Yates, tudo com RandomNumberGenerator.GetInt32. System.Random não é usado.
- Política: 16–128 caracteres, só a-z A-Z 0-9 e !@#$%^&\*()-\_=+[]{};:,.<>?/, sem espaços, com maiúscula, minúscula, dígito e especial (RN14).
- Configuração: Length=24 e ExcludeAmbiguous=false. A aplicação não arranca com Length fora de 16–128 (ValidateOnStart).
- Persistência: CHECK (length(Senha) BETWEEN 16 AND 128), Migrate() e WAL no arranque, senha em texto claro (sem cifragem, D07).
- GUID: Guid.NewGuid() (v4). Colisão de PK (código 1555) repete até 3 vezes. Falha de gravação dá 500 sem GUID nem registo.
- Logs: Serilog JSON, request logging pelo route template, sem RequestPath, sem corpos, sem senha nem GUID bruto.
- Métricas: contadores passwords.generated e passwords.retrieved.

Neste processo eu aprendi:

- ✅ Oranização e estrutura dos arefeactos
- ✅ como funcionam a organização por baixo dos panos em frameworks de sdd

## Bagde

<img src="https://baltaio.blob.core.windows.net/static/images/v4/challenges/halloween-2026/01.png" width="200" />

## Problema

A criação manual de senhas fortes é um processo lento, inconsistente e propenso a erros. Os utilizadores podem gerar senhas curtas, previsíveis ou que não respeitam os requisitos mínimos de segurança definidos pelas aplicações, aumentando o risco de acessos não autorizados.

Além disso, a ausência de um mecanismo centralizado para gerar senhas com critérios configuráveis dificulta a aplicação consistente de políticas de segurança entre diferentes sistemas.

## Sobre o Halloween 2026

O desafio **Halloween 2026** consiste em implementar implementar o modelo Spec Driven Development de ponta a ponta, criando apps completas com IA.

### Veja meu progresso no desafio

[Incluir link para o repositório central]
