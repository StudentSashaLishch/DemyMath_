# DemyMath Architecture Description

## 1. Purpose

This document defines the architecture of the **DemyMath** web information system.

It establishes architectural structure, boundaries, responsibilities, interfaces, constraints, and principal architectural decisions. It does not prescribe implementation details that do not affect the defined architecture.

## 2. System Context

DemyMath is a web system for interactive mathematics learning.

The system consists of:

* a client part operating in a web browser;
* a server part responsible for authoritative processing and persistent learning state;
* persistent data storage.

The primary external actor is the **Student**.

The **Administrator** manages educational content and performs administrative operations.

The client communicates with the server through defined application interfaces. The server is authoritative for mathematical validity, learning state, skill state, topic status, and other state-changing operations.

## 3. Architectural Concerns

The architecture addresses the following concerns:

* separation of presentation and domain behavior;
* mathematical correctness independent of the user interface;
* authoritative server-side state management;
* separation of learning-model responsibilities;
* persistence of learning state;
* extensibility of mathematical operations and skills;
* testability of domain behavior;
* maintainability of the web application;
* protection of user learning data.

## 4. Architectural Principles

### AP-01 Client-Server Separation

The client shall provide presentation and user interaction.

The server shall perform authoritative processing and state-changing operations.

### AP-02 MVC

The web application shall use the Model-View-Controller pattern.

Controllers shall coordinate HTTP requests and application operations.

Views shall provide presentation.

Domain rules shall not be implemented in controllers or views.

### AP-03 Domain Independence

Mathematical correctness and learning rules shall be independent of web interface implementation.

The domain shall not depend on controllers, views, client-side code, or database-specific implementation details.

### AP-04 Separation by Responsibility

Components shall be separated according to distinct responsibilities.

A component may be implemented as a separate module when its responsibility, isolation, or reuse justifies the separation.

The architecture does not require independent deployment of individual modules.

### AP-05 Server Authority

Client-side validation may support interaction but shall not determine authoritative application state.

The server shall verify state-changing operations before accepting them.

### AP-06 Explicit Interfaces

Communication between architectural components shall occur through defined interfaces or contracts.

A component shall not directly access implementation details belonging to another architectural responsibility without architectural justification.

## 5. Logical Architecture

The logical architecture consists of the following responsibilities:

```text
Client
  │
  ▼
Web / Presentation
  │
  ▼
Application
  │
  ├──────────────► Mathematical Expression Module
  │
  ├──────────────► Learning Model
  │
  └──────────────► Domain Rules
  │
  ▼
Persistence
  │
  ▼
Data Storage
```

### 5.1 Client

The client provides:

* user interface;
* interaction with learning materials and exercises;
* presentation of mathematical expressions;
* presentation of results, statuses, and hints;
* transmission of user actions to the server.

The client shall not be the authoritative source of learning state or mathematical correctness.

### 5.2 Web / Presentation

The presentation layer provides HTTP interaction and MVC-based presentation.

It translates external requests into application operations and application results into responses suitable for the client.

It shall not contain authoritative domain rules.

### 5.3 Application

The application layer coordinates system operations and controls interaction between presentation, domain responsibilities, mathematical processing, and persistence.

It shall not replace domain rules with presentation-specific logic.

### 5.4 Domain

The domain contains rules governing:

* mathematical operations;
* exercise behavior;
* attempts and results;
* skill states;
* topic states;
* repetition;
* adaptive exercise selection;
* reflection;
* learning progress.

Domain behavior shall remain independent of infrastructure and presentation mechanisms.

### 5.5 Mathematical Expression Module

The mathematical expression module is responsible for:

* structured representation of mathematical expressions;
* supported mathematical operations;
* operation preconditions;
* mathematical validation;
* resulting expression states;
* preservation of required mathematical properties.

The module shall not depend on MVC components or specific client interaction mechanisms.

### 5.6 Learning Model

The learning model is responsible for processing educational results and determining:

* skill-state changes;
* topic-state changes;
* repetition requirements;
* subsequent exercise selection.

The learning model shall use defined learning data and shall not depend on presentation-specific behavior.

### 5.7 Persistence

The persistence responsibility provides storage and retrieval of authoritative learning state and other required system data.

Persistence implementation details shall remain outside domain logic.

## 6. Runtime Interaction

A typical state-changing operation follows this logical sequence:

```text
Client
  → Web / Presentation
  → Application
  → Domain / Mathematical Module
  → Persistence
  → Application
  → Web / Presentation
  → Client
```

The server shall validate the operation before committing a resulting state.

An unsuccessful validation shall not produce an accepted state transition.

## 7. Data and State Ownership

The server owns authoritative:

* learning state;
* skill state;
* topic status;
* exercise results;
* repetition schedule;
* reflection data;
* mathematical expression state associated with an active server-managed operation, where persistence is required.

The client may maintain temporary interaction state but shall not define authoritative outcomes.

## 8. Deployment Architecture

The system shall be deployable as a web application consisting of:

```text
User Device
└── Web Browser
        │
      HTTPS
        │
        ▼
Server Environment
└── DemyMath Web Application
        │
        ▼
    Data Storage
```

The client requires no specialized software installation.

The server may operate on dedicated hardware or in a cloud environment satisfying the system's technical requirements.

The architecture does not require microservices or independent deployment of logical modules.

## 9. Architectural Constraints

The architecture is constrained by:

* ASP.NET Core MVC;
* C# as the primary server-side language;
* browser-based client operation;
* HTTPS communication;
* persistent storage of authorized learning state;
* server-side authorization and validation;
* independence of mathematical verification from the web interface.

Specific libraries, database mappings, and internal implementation structures are not architectural requirements unless they affect these constraints.

## 10. Extensibility

The architecture shall permit extension of:

* mathematical operations;
* mathematical object types;
* skills;
* exercise types;
* learning-model rules.

An extension shall not require modification of unrelated existing processing logic.

Logical separation shall be preferred where it reduces coupling between independently changing responsibilities.

## 11. Architectural Views

The architecture is described through the following views:

| View            | Purpose                                                             |
| --------------- | ------------------------------------------------------------------- |
| Context view    | Defines system boundary, external actors, and external interactions |
| Logical view    | Defines major responsibilities and their relationships              |
| Runtime view    | Defines principal interaction and state-changing flows              |
| Deployment view | Defines client, server, communication, and storage nodes            |

These views address the principal stakeholder concerns without prescribing implementation details that are not architecturally significant.

## 12. Architectural Decisions

The following decisions are architectural constraints for the first version:

| Decision                                                 | Rationale                                                                                 |
| -------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| Client-server architecture                               | Separates user interaction from authoritative processing and persistent state             |
| MVC                                                      | Separates HTTP coordination, presentation, and application/domain behavior                |
| Separate mathematical expression responsibility          | Prevents mathematical correctness from depending on web presentation                      |
| Separate learning-model responsibility                   | Isolates adaptive learning behavior from presentation and persistence                     |
| Server-authoritative state                               | Prevents client-side manipulation of learning outcomes                                    |
| Logical modularity without mandatory separate deployment | Provides separation of responsibilities without unnecessary distributed-system complexity |

## 13. Architecture Conformance

An implementation conforms to this architecture when:

* the defined architectural responsibilities remain separated;
* domain rules are independent of presentation and infrastructure;
* mathematical correctness is independent of web interface components;
* state-changing operations are authoritative on the server;
* defined component boundaries and interfaces are respected;
* architectural constraints are not violated without an approved architectural change.