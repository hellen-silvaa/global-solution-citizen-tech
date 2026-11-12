# Evidências da aplicação

Esta pasta reúne as evidências da API Oportuniza. Dizemos aqui, com franqueza, o que cada arquivo é.

## 1. Capturas do Swagger UI (`*.png`)

São capturas da **documentação** do Swagger (`/swagger`): cada endpoint expandido, com resumo, parâmetros, códigos de resposta e **exemplos de request/response**. **Não são requisições executadas** ("Try it out" + Execute).

- Os principais estão no [README principal](../README.md#11-evidências) com explicação: `01a`, `01b`, `02` a `08`.
- Os demais arquivos `Captura de tela 2026-11-11 *.png` são um print de cada um dos outros endpoints (cadastro de organização, inscrições, oportunidades, organizações, propostas etc.). Alguns estão pequenos ou repetidos.
- Os exemplos de erro dessas capturas foram tirados **antes** de uma correção: o corpo de erro mostrava a mensagem de "inscrição duplicada" em todos os códigos. Hoje o exemplo é neutro ("Mensagem em português explicando o motivo do erro."). A documentação vigente é a de [`docs/openapi.json`](../docs/openapi.json).

## 2. Saídas reais capturadas por comando (`*.txt`)

Geradas a partir da API rodando no Docker e do código, em 11/11/2026:

| Arquivo | O que prova |
|---|---|
| [`07-indicadores-resposta-real.txt`](07-indicadores-resposta-real.txt) | `GET /indicadores/impacto` com resposta real (JSON) |
| [`08-viacep.txt`](08-viacep.txt) | `GET /enderecos/01001-000`, integração real com o ViaCEP |
| [`09-mysql-migracoes-tabelas.txt`](09-mysql-migracoes-tabelas.txt) | As 6 migrações em `__EFMigrationsHistory` e as tabelas no MySQL |
| [`10-dotnet-test.txt`](10-dotnet-test.txt) | `dotnet test`: 338 aprovados, 0 falhas; cobertura de linhas 100% e de branches 98,6% |
| [`12-cabecalhos-seguranca.txt`](12-cabecalhos-seguranca.txt) | Cabeçalhos de segurança da resposta (CSP, X-Frame-Options, nosniff, Referrer-Policy...) |

## 3. Front-end (`front/`)

28 capturas geradas com Playwright: 7 telas (home, oportunidades, propostas, organizações, impacto, login, cadastro), em tema claro e escuro, desktop e celular. Mais os 19 testes E2E que percorrem os fluxos contra a API real (ver `front/README.md`).
