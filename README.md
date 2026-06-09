# MotoRev

## Sumário

- [1. Visão Geral](#1-visão-geral)
- [2. Objetivo do Projeto](#2-objetivo-do-projeto)
- [3. Arquitetura](#3-arquitetura)
- [4. Tecnologias Utilizadas](#4-tecnologias-utilizadas)
- [5. Estrutura do Repositório](#5-estrutura-do-repositório)
- [6. Funcionalidades (Requisitos Funcionais)](#6-funcionalidades-requisitos-funcionais)
- [7. Regras de Negócio](#7-regras-de-negócio)
- [8. Autenticação e Autorização](#8-autenticação-e-autorização)
- [9. Banco de Dados](#9-banco-de-dados)
- [10. Testes](#10-testes)
- [11. Como Executar o Projeto](#11-como-executar-o-projeto)
- [12. API](#12-api)
- [13. Documentação UML e Arquitetura](#13-documentação-uml-e-arquitetura)
- [14. Organização Ágil](#14-organização-ágil)
- [15. Autores](#15-autores)

---

## 1. Visão Geral

O MotoRev é um sistema web para gerenciamento de revisões de motocicletas, permitindo o controle de clientes, motos, concessionárias, agendamentos, execuções de revisões e histórico de manutenção.

O sistema possui dois perfis principais de uso:

- **Cliente**: usuário proprietário de uma ou mais motocicletas.
- **Concessionária**: usuário responsável por organizar, atender e executar revisões.

Após a autenticação, o sistema identifica o perfil do usuário e libera as funcionalidades adequadas conforme suas permissões.

---

## 2. Objetivo do Projeto

O objetivo do projeto é aplicar conceitos de desenvolvimento de software, incluindo arquitetura em camadas, modelagem de domínio, uso de ORM, APIs REST, autenticação, testes automatizados, documentação técnica e práticas ágeis.

---

## 3. Arquitetura

O sistema foi desenvolvido utilizando arquitetura em camadas:

- **Presentation (API)**: Controllers e endpoints REST.
- **Application**: Services, regras de negócio e orquestração dos fluxos.
- **Domain**: Models, Enums e entidades principais do domínio.
- **Infrastructure**: Persistência com Entity Framework Core, SQL Server e configurações de banco.

Cada camada possui responsabilidade única e comunicação bem definida.

---

## 4. Tecnologias Utilizadas

### Back-end
- .NET
- ASP.NET Core Web API
- ASP.NET Core Identity
- Entity Framework Core (Code First)
- Mapster (Mapeamento de objetos)
- xUnit
- Coverlet
- Moq (Mocking para testes unitários)

### Banco de Dados
- SQL Server
- EF Core Migrations
- LINQ

### Front-end
- React 19
- Vite 7
- TypeScript
- HTML
- Ant Design v6

### DevOps e Ambiente
- Docker
- Docker Compose
- Git
- GitHub Projects

### Documentação
- OpenAPI / Scalar API Reference
- UML
- BDD em critérios de aceite

---

## 5. Estrutura do Repositório

```text
motorev/
├─ client/
│  ├─ public/
│  ├─ src/
│  ├─ index.html
│  ├─ package.json
│  ├─ tsconfig.json
│  └─ vite.config.ts
│
├─ server/
│  ├─ MotoRevApi/
│  │  ├─ Authorization/
│  │  ├─ Controller/
│  │  ├─ Data/
│  │  ├─ Dto/
│  │  │  ├─ Request/
│  │  │  └─ Response/
│  │  ├─ Enums/
│  │  ├─ Exceptions/
│  │  ├─ Handlers/
│  │  ├─ Migrations/
│  │  ├─ Model/
│  │  ├─ Profiles/
│  │  ├─ Properties/
│  │  ├─ Providers/
│  │  ├─ Services/
│  │  ├─ appsettings.json
│  │  ├─ appsettings.Development.json
│  │  ├─ MotoRevApi.csproj
│  │  ├─ MotoRevApi.http
│  │  └─ Program.cs
│  │
│  └─ MotoRevApi.Tests/        # Testes automatizados (xUnit, Moq, InMemoryDb)
│
├─ Docs/
├─ PrintScreen/
├─ compose.yaml
├─ LICENSE
├─ MotoRev.slnx
└─ README.md
```

---

## 6. Funcionalidades (Requisitos Funcionais)

- RF-1: Autenticação e controle de acesso.
- RF-2: Gestão de clientes.
- RF-3: Gestão de concessionárias.
- RF-4: Gestão de motos.
- RF-5: Catálogo de modelos de motos.
- RF-6: Catálogo de peças.
- RF-7: Catálogo de serviços.
- RF-8: Catálogo de revisões.
- RF-9: Agendamento de revisões.
- RF-10: Registro de execução de revisões.
- RF-11: Consulta de histórico de revisões.
- RF-12: Gestão de endereços.
- RF-13: Geração de alertas.
- RF-14: Auditoria de operações.
- RF-15: Disponibilização de dados por API.

---

## 7. Regras de Negócio

- Uma moto deve estar vinculada a um cliente.
- Um cliente pode possuir uma ou mais motos.
- Uma concessionária pode receber solicitações de agendamento.
- Um agendamento deve estar associado a uma moto, cliente, concessionária e revisão prevista.
- Revisões executadas podem referenciar um agendamento.
- Peças e serviços utilizados devem existir previamente no catálogo.
- Revisões padrão são compostas por peças e serviços cadastrados.
- Chassi e placa devem ser únicos.
- Registros inativados não devem ser removidos fisicamente quando houver histórico associado.

---

## 8. Autenticação e Autorização

O sistema utiliza **ASP.NET Core Identity** para gerenciamento de autenticação, usuários, credenciais e controle de acesso.

A autenticação considera dois perfis principais:

- **Cliente**
- **Concessionária**

O perfil autenticado é utilizado para definir:

- permissões de acesso;
- redirecionamento após login;
- funcionalidades disponíveis na interface;
- restrição de endpoints;
- regras de visualização e operação.

---

## 9. Banco de Dados

O sistema utiliza SQL Server com abordagem Code First por meio do Entity Framework Core.

### Estratégias adotadas

- Migrations para versionamento do banco.
- Models persistidos via EF Core.
- Relacionamentos entre entidades do domínio.
- Uso de LINQ para consultas.
- Possibilidade de consultas dinâmicas para relatórios e dashboards.

### Índices previstos

Campos candidatos a índice:

- Email do usuário.
- CPF do cliente.
- CNPJ da concessionária.
- Placa da moto.
- Chassi da moto.
- Status de agendamento.
- Data de agendamento.
- Data de execução da revisão.

---

## 10. Testes

O projeto utiliza **xUnit** para testes automatizados no back-end.

A cobertura de testes é acompanhada com **Coverlet**, permitindo medir a cobertura dos testes unitários.

### Comando base para executar testes

```bash
dotnet test
```

### Comando para executar testes com cobertura

```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings
```

Esse comando gera um arquivo `coverage.cobertura.xml` dentro da pasta `TestResults` do projeto de testes. O caminho costuma seguir este formato:

```text
server/MotoRevApi.Tests/TestResults/{guid}/coverage.cobertura.xml
```

O arquivo `coverlet.runsettings` exclui da análise de cobertura os arquivos que não representam regras de negócio testáveis, como:

- `Program.cs`;
- configurações de OpenAPI geradas;
- migrations do Entity Framework Core;
- arquivos gerados em `obj`;
- configuração de mapeamento `Profiles/MapsterConfig.cs`.

### Gerar relatório HTML da cobertura

Após gerar o XML de cobertura, utilizar o **ReportGenerator** para criar uma visualização em HTML:

```bash
reportgenerator -reports:"server/MotoRevApi.Tests/TestResults/{guid}/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

Substituir `{guid}` pelo nome da pasta criada dentro de `TestResults` na execução do `dotnet test`.

O relatório HTML será gerado em:

```text
coveragereport/index.html
```

Abrir esse arquivo no navegador para visualizar o relatório de cobertura.

Sempre que os testes ou o código do back-end forem alterados, executar novamente o comando de testes com cobertura e depois gerar novamente o relatório HTML.

### Objetivo dos testes

- Validar regras de negócio nos Services.
- Validar cenários críticos de autenticação.
- Validar cadastros, consultas, atualizações e inativações.
- Validar regras dos catálogos.
- Validar fluxos de agendamento e execução de revisões.
- Apoiar o acompanhamento de cobertura mínima definida para o projeto.

---

## 11. Como Executar o Projeto

### Pré-requisitos

- Docker
- .NET SDK
- Node.js

### Passos

```bash
# Navegar até a API
cd ../server/MotoRevApi

# Subir infraestrutura
docker compose up

# Build do projeto
dotnet build

# Executar API
dotnet run
```

### Configuração do Entity Framework Core

Instalar a ferramenta do EF Core na primeira execução:

```bash
dotnet tool install --global dotnet-ef
```

### Migrations

```bash
dotnet ef migrations add nome-da-sua-migration
dotnet ef database update
```

### Carga de dados de desenvolvimento

Para aplicar migrations e executar as cargas em ordem:

```powershell
.\scripts\seed-database.ps1
```

Ordem executada:

```text
seed-pecas-servicos.sql
seed-linhas.sql
seed-modelos-motos.sql
seed-clientes-motos.sql
seed-concessionarias.sql
```

As concessionarias criadas por `seed-concessionarias.sql` usam a senha padrao:

```text
Concessionaria@123
```

Cada concessionaria seed possui 5 lojas: 1 matriz e 4 filiais.

Os clientes criados por `seed-clientes-motos.sql` usam a senha padrao:

```text
Cliente@123
```

Para executar junto com o prelaunch:

```powershell
.\scripts\prelaunch.ps1 -SeedDatabase
```

Ou habilite por variavel de ambiente:

```powershell
$env:MOTOREV_SEED_DATABASE = "true"
.\scripts\prelaunch.ps1
```

---

## 12. API

A API REST do projeto está em construção.

A documentação dos endpoints será disponibilizada via OpenAPI / Scalar, contemplando:

- endpoints disponíveis;
- métodos HTTP;
- modelos de request;
- modelos de response;
- códigos de erro;
- exemplos de uso.

---

## 13. Documentação UML e Arquitetura

A documentação UML do projeto está disponível na pasta `Docs`.

### Diagramas disponíveis

#### Caso de Uso

![Diagrama de Caso de Uso](https://github.com/rdsalvesPUC/motorev/blob/doc/uml-e-arquitetura/Docs/UseCase%20Diagram0.jpg?raw=true)

#### Diagrama de Classes — Usuários e Acesso

![Diagrama de Classes - Usuários e Acesso](https://github.com/rdsalvesPUC/motorev/blob/doc/uml-e-arquitetura/Docs/ClassDiagram%20-%20Usu%C3%A1rios%20e%20Acesso.jpg?raw=true)

#### Diagrama de Classes — Catálogos e Revisões Padrão

![Diagrama de Classes - Catálogos e Revisões Padrão](https://github.com/rdsalvesPUC/motorev/blob/doc/uml-e-arquitetura/Docs/ClassDiagram%20-%20Cat%C3%A1logos%20e%20Revis%C3%B5es%20Padr%C3%A3o.jpg?raw=true)

#### Diagrama de Classes — Agendamento e Execução de Revisões

![Diagrama de Classes - Agendamento e Execução de Revisões](https://github.com/rdsalvesPUC/motorev/blob/doc/uml-e-arquitetura/Docs/ClassDiagram%20-%20Agendamento%20e%20Execu%C3%A7%C3%A3o%20de%20Revis%C3%B5es.jpg?raw=true)

#### Diagrama de Atividade — Autenticação

![Diagrama de Atividade - Autenticação](https://github.com/rdsalvesPUC/motorev/blob/doc/uml-e-arquitetura/Docs/ActDiagram%20-%20Autenticacao.jpg?raw=true)

#### Diagrama de Atividade — Agendamento de Revisão

![Diagrama de Atividade - Agendamento de Revisão](https://github.com/rdsalvesPUC/motorev/blob/doc/uml-e-arquitetura/Docs/ActDiagram%20-%20Agendamento%20de%20Revisao.jpg?raw=true)

#### Diagrama de Sequência — Agendamento de Revisão

![Diagrama de Sequência - Agendamento de Revisão](https://github.com/rdsalvesPUC/motorev/blob/doc/uml-e-arquitetura/Docs/SeqDiagram%20-%20Agendamento%20de%20Revisao.jpg?raw=true)

#### Diagrama de Sequência — Executar Revisão Agendada

![Diagrama de Sequência - Executar Revisão Agendada](https://github.com/rdsalvesPUC/motorev/blob/doc/uml-e-arquitetura/Docs/SeqDiagram%20-%20Executar%20Revisao%20Agendada.jpg?raw=true)

### Arquivos complementares

- Documento de arquitetura: `Docs/DA - MotoRev v2.docx`
- Arquivo Astah: `Docs/MotoRev UML.asta`
- Planilha de avaliação: `Docs/Avaliação_ExpCriativa2026.xlsx`

---

## 14. Organização Ágil

O projeto foi organizado utilizando práticas ágeis:

- Epics para grandes módulos do sistema.
- User Stories para funcionalidades.
- Tasks técnicas por camada.
- Critérios de aceite no formato BDD.
- Registro das entregas no GitHub Projects.

### Board do Projeto

https://github.com/users/rdsalvesPUC/projects/3

### Exemplo de critério de aceite

```text
Cenário: Agendamento válido
- Dado uma moto cadastrada
- Quando o cliente agenda uma revisão
- Então o sistema deve registrar o agendamento
```

---

## 15. Autores

- Rodrigo Alves
- Marco Alija
- Richard Mickael

PUC-PR / Sistemas de Informação
