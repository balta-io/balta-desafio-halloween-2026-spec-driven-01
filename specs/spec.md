# Gerador de Senhas Fortes

## 1. Problema

A criação manual de senhas fortes é um processo lento, inconsistente e propenso a erros. Os utilizadores podem gerar senhas curtas, previsíveis ou que não respeitam os requisitos mínimos de segurança definidos pelas aplicações, aumentando o risco de acessos não autorizados.

Além disso, a ausência de um mecanismo centralizado para gerar senhas com critérios configuráveis dificulta a aplicação consistente de políticas de segurança entre diferentes sistemas.

Este Solução propõe de uma API de geração de senhas que permita criar senhas aleatórias, fortes e configuráveis, de forma rápida e padronizada, garantindo o cumprimento dos requisitos de segurança definidos. A solução deverá também estabelecer uma base para futuras funcionalidades de gestão segura de credenciais, sem comprometer a confidencialidade das senhas geradas.

## 2. Objetivo

Entregar uma API com dois endpoints: um gera uma senha forte segundo um padrão fixo, guarda-a na base de dados e devolve um GUID; o outro devolve a senha associada a um GUID.
Considera-se concluído quando as duas operações funcionam de ponta a ponta, a política de senha é imposta em todas as gerações e todos os critérios de aceitação da secção 9 passam.

## 3. Usuários

| usuário           | Descrição                                                          | Endpoints que usa |
| ----------------- | ------------------------------------------------------------------ | ----------------- |
| Aplicação cliente | Sistema que precisa de uma senha forte e de a recuperar mais tarde | Gerar e consultar |
| Programador       | Quem integra, testa e depura a API                                 | Gerar e consultar |

## 4. Histórias

| ID  | História                                                                                                  |
| --- | --------------------------------------------------------------------------------------------------------- |
| H1  | Como aplicação cliente, quero pedir uma senha forte, para não ter de a inventar.                          |
| H2  | Como aplicação cliente, quero receber um GUID ao gerar a senha, para a poder encontrar mais tarde.        |
| H3  | Como aplicação cliente, quero consultar a senha através do GUID, para a usar quando for necessária.       |
| H4  | Como programador, quero erros claros para GUID inválido ou inexistente, para tratar falhas sem adivinhar. |

## 5. Requisitos funcionais

| ID   | Requisito                                                                                               |
| ---- | ------------------------------------------------------------------------------------------------------- |
| RF01 | `POST /api/v1/passwords` gera uma senha que cumpre a política da secção 6, sem corpo no pedido.         |
| RF02 | O `POST` persiste a senha na base de dados antes de responder.                                          |
| RF03 | O `POST` responde `201 Created` com o GUID gerado (`{ "id": "<guid>" }`) e não devolve a senha.         |
| RF04 | `GET /api/v1/passwords/{guid}` devolve `200 OK` com a senha associada (`{ "senha": "<valor>" }`).       |
| RF05 | O `GET` com GUID mal formado responde `400 Bad Request`.                                                |
| RF06 | O `GET` com GUID bem formado mas inexistente responde `404 Not Found`.                                  |
| RF07 | Cada chamada ao `POST` produz uma senha nova e um GUID novo.                                            |
| RF08 | O mesmo GUID devolve sempre a mesma senha.                                                              |
| RF09 | Erros usam um formato único e consistente (por exemplo, `ProblemDetails`), sem expor detalhes internos. |

## 6. Regras de negócio

| ID   | Regra                               | Descrição                                                                                                                                                                                                                         | Origem                 |
| ---- | ----------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------- |
| RN01 | Comprimento mínimo                  | A senha gerada deve conter, no mínimo, 16 caracteres.                                                                                                                                                                             | Enunciado              |
| RN02 | Comprimento máximo                  | A senha gerada não pode exceder 128 caracteres.                                                                                                                                                                                   | Requisito técnico      |
| RN03 | Presença de caracteres especiais    | A senha deve conter, obrigatoriamente, pelo menos um carácter especial pertencente ao conjunto permitido pela API: `!@#$%^&*()-_=+[]{};:,.<>?/`.                                                                                  | Enunciado              |
| RN04 | Ausência de espaços em branco       | A senha não pode conter espaços, tabulações, quebras de linha ou outros caracteres classificados como espaços em branco.                                                                                                          | Enunciado              |
| RN05 | Conjuntos de caracteres permitidos  | A senha deve utilizar exclusivamente caracteres dos seguintes conjuntos: minúsculas (`abcdefghijklmnopqrstuvwxyz`), maiúsculas (`ABCDEFGHIJKLMNOPQRSTUVWXYZ`), dígitos (`0123456789`) e especiais (`!@#$%^&*()-_=+[]{};:,.<>?/`). | Requisito técnico      |
| RN06 | Aleatoriedade criptográfica         | A geração da senha deve utilizar `RandomNumberGenerator` do .NET, sendo proibida a utilização de `System.Random` para este fim.                                                                                                   | Requisito de segurança |
| RN07 | Exclusão de caracteres ambíguos     | A API deve permitir excluir opcionalmente os caracteres `I`, `l`, `1`, `O` e `0` da geração. Quando ativada, esta opção deve garantir que nenhum desses caracteres apareça na senha.                                              | Proposta               |
| RN08 | Validação da composição             | Cada senha gerada deve respeitar o comprimento solicitado, os conjuntos de caracteres permitidos, a presença obrigatória de pelo menos um carácter especial e a ausência de espaços em branco.                                    | Regra derivada         |
| RN09 | Persistência obrigatória            | Cada senha gerada com sucesso deve ser armazenada na base de dados antes de a API devolver uma resposta de sucesso.                                                                                                               | Enunciado              |
| RN10 | Identificador único                 | Cada senha armazenada deve possuir um GUID gerado pelo servidor, independente do conteúdo da senha, utilizado como identificador único do registo.                                                                                | Proposta               |
| RN11 | Recuperação por identificador       | A API deve permitir recuperar uma senha previamente armazenada através do respetivo GUID, mediante os controlos de acesso definidos.                                                                                              | Enunciado              |
| RN12 | Consistência da operação            | Se a persistência falhar, a API não deve devolver uma resposta de sucesso nem disponibilizar um GUID correspondente a um registo inexistente ou não persistido.                                                                   | Proposta               |
| RN13 | Imutabilidade                       | Uma senha armazenada não pode ser alterada após a sua criação. A geração de uma nova senha deve originar um novo registo e um novo GUID.                                                                                          | Proposta               |
| RN14 | Proteção das senhas armazenadas     | As senhas devem ser cifradas em repouso com um mecanismo de cifragem autenticada, como AES-GCM, utilizando uma chave gerida separadamente da base de dados. As senhas não devem ser registadas nos logs da aplicação.             | Requisito de segurança |
| RN15 | Validação do comprimento solicitado | Quando o consumidor indicar um comprimento inferior a 16 ou superior a 128 caracteres, a API deve rejeitar o pedido com uma resposta HTTP `400 Bad Request`.                                                                      | Proposta               |
| RN16 | Garantia de composição reforçada    | Caso seja adotada a política de composição reforçada, a senha deve conter pelo menos uma letra maiúscula, uma letra minúscula e um dígito, além de pelo menos um carácter especial.                                               | Proposta a confirmar   |

### Conjuntos de caracteres permitidos

| Tipo                                        | Conjunto de caracteres       |
| ------------------------------------------- | ---------------------------- |
| Minúsculas                                  | `abcdefghijklmnopqrstuvwxyz` |
| Maiúsculas                                  | `ABCDEFGHIJKLMNOPQRSTUVWXYZ` |
| Dígitos                                     | `0123456789`                 |
| Caracteres especiais                        | `!@#$%^&*()-_=+[]{};:,.<>?/` |
| Caracteres ambíguos a excluir opcionalmente | `Il1O0`                      |

## 7. Casos de borda

| Situação                                            | Comportamento esperado                                                          |
| --------------------------------------------------- | ------------------------------------------------------------------------------- |
| GUID mal formado (texto livre, comprimento errado)  | `400 Bad Request`, sem consultar a base de dados                                |
| GUID vazio (`00000000-0000-0000-0000-000000000000`) | `400 Bad Request`                                                               |
| GUID válido que não existe                          | `404 Not Found`, sem indicar se algum GUID parecido existe                      |
| GUID em maiúsculas, minúsculas ou sem hífenes       | Normalizado por `Guid.TryParse`; devolve a mesma senha                          |
| Gerador produz uma senha sem carácter obrigatório   | Nunca se devolve; a senha é construída para garantir cada classe, ou regenerada |
| Falha de ligação ou gravação na base de dados       | `500`, sem GUID devolvido e sem registo parcial                                 |
| Colisão de GUID (praticamente impossível)           | A chave primária rejeita; repete-se a geração um número limitado de vezes       |
| Pedidos `POST` em simultâneo                        | Cada um recebe senha e GUID distintos, sem partilha de estado                   |
| Senha com aspas, barras ou `\`                      | Serializada em JSON com escape correcto, sem alterar o valor                    |
| Tentativa de adivinhar GUIDs em massa               | Sem limite de taxa neste desafio; risco assumido (secção 8)                     |
| Método ou `Content-Type` não suportado              | `405 Method Not Allowed` ou `415 Unsupported Media Type`                        |

## 8. Fora de escopo

- Autenticação e autorização nos endpoints.
- Limite de taxa e protecção contra enumeração de GUIDs.
- Alterar, eliminar ou expirar senhas, e listar as já geradas.
- Parâmetros do cliente sobre a senha (comprimento, conjuntos de caracteres, número de senhas por pedido).
- Rotação de chaves, KMS ou HSM; a cifra em repouso (secção 6) usa uma chave simples e configurável.
- Auditoria e registo de quem consultou cada senha.
- Interface gráfica, multi-tenant, versionamento da API e deployment.

## 9. Critérios de aceite

| ID   | Dado / Quando / Então                                                                                                                                                                   |
| ---- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| CA01 | **Dado** um pedido `POST /api/v1/passwords`, **quando** é processado, **então** responde `201` com um GUID válido no corpo.                                                             |
| CA02 | **Dado** uma senha gerada, **quando** se consulta a base de dados, **então** existe um registo associado ao GUID devolvido.                                                             |
| CA03 | **Dado** 1000 senhas geradas, **quando** se validam, **então** todas têm pelo menos 16 caracteres.                                                                                      |
| CA04 | **Dado** 1000 senhas geradas, **quando** se validam, **então** todas têm pelo menos um carácter especial do conjunto documentado.                                                       |
| CA05 | **Dado** 1000 senhas geradas, **quando** se validam, **então** nenhuma contém espaço em branco.                                                                                         |
| CA06 | **Dado** 1000 senhas geradas, **quando** se comparam, **então** não há duas iguais e não há GUIDs repetidos.                                                                            |
| CA07 | **Dado** um GUID devolvido pelo `POST`, **quando** se chama `GET /api/v1/passwords/{guid}`, **então** responde `200` com exactamente a senha gerada.                                    |
| CA08 | **Dado** o mesmo GUID, **quando** se chama o `GET` várias vezes, **então** a senha devolvida é sempre a mesma.                                                                          |
| CA09 | **Dado** um GUID bem formado que não existe, **quando** se chama o `GET`, **então** responde `404`.                                                                                     |
| CA10 | **Dado** um identificador que não é GUID, **quando** se chama o `GET`, **então** responde `400`.                                                                                        |
| CA11 | **Dado** uma falha simulada na gravação, **quando** se chama o `POST`, **então** responde `500` e não existe registo nem GUID devolvido.                                                |
| CA12 | **Dado** a base de dados, **quando** se inspecciona a coluna da senha, **então** o valor não está em texto claro (se a história H5 e a decisão de segurança da secção 6 forem aceites). |
