# DemyMath System Requirements

## 1. Purpose

This document defines the system-level requirements for the automated interactive mathematics learning system **DemyMath**.

The document establishes the required system behavior, quality constraints, external constraints, and requirements attributes. Detailed implementation decisions are outside its scope.

## 2. Scope

DemyMath is a web information system for interactive mathematics learning.

The system shall support:

* navigation through mathematical topics;
* access to theoretical and practical learning materials;
* interactive work with mathematical expressions;
* verification of mathematical operations and answers;
* contextual hints;
* learning progress and skill-state tracking;
* spaced repetition;
* adaptive exercise selection;
* control exercises;
* reflection;
* preservation of authorized users' learning state.

The system shall operate through a web browser using a client-server architecture.

## 3. Stakeholders

| Stakeholder    | Concern                                                                                |
| -------------- | -------------------------------------------------------------------------------------- |
| Student        | Access to learning materials, exercises, feedback, progress, and personalized practice |
| Administrator  | Management of educational content and system data                                      |
| Developer      | Correctness, maintainability, testability, and extensibility of the system             |
| Project author | Compliance with the technical specification and defined acceptance criteria            |

## 4. Requirement Classification

Requirements are classified as:

* functional requirements;
* quality requirements;
* interface and compatibility requirements;
* architectural constraints.

Each normative requirement shall have a unique identifier and shall be stated as an observable system obligation.

## 5. Functional Requirements

### FR-01 Learning Navigation

The system shall represent learning topics and their educational relationships as a navigable learning graph.

The system shall provide a status for each topic based on the defined learning-state model.

Prerequisites shall not prevent access to a topic.

### FR-02 Learning Content

The system shall provide theoretical materials and practical exercises associated with topics.

Theoretical materials shall remain accessible while exercises are performed.

### FR-03 Mathematical Expressions

The system shall represent supported mathematical expressions as structured objects.

The system shall allow the user to select supported elements or subexpressions and perform applicable mathematical operations.

Each accepted operation shall produce a new expression state.

### FR-04 Mathematical Verification

The system shall verify each mathematical operation against its defined applicability conditions and mathematical correctness rules before accepting the resulting state.

An invalid operation shall not modify the current expression state.

### FR-05 Operation History

The system shall maintain the sequence of accepted mathematical operations associated with the corresponding learning activity.

The system shall support undo and redo of accepted operations.

### FR-06 Hints

The system shall provide contextual hints based on the current exercise and mathematical expression state.

The system shall distinguish the defined hint levels and record hint usage.

A hint shall not modify the actual exercise state unless explicitly defined as part of the exercise behavior.

### FR-07 Skill State

The system shall maintain the state of individual mathematical skills independently of the generalized topic status.

Skill state shall be updated using defined educational results, including exercise correctness, errors, attempts, hints, and control results where applicable.

### FR-08 Exercise Selection

The system shall select subsequent training exercises using the current skill state, previous results, and repetition requirements.

Adaptive selection shall affect exercise choice and order and shall not restrict mathematically valid solution paths.

### FR-09 Spaced Repetition

The system shall schedule repetition of topics and skills using previous learning results.

The repetition interval shall depend on learning performance and shall be adjustable according to the defined learning model.

### FR-10 Control Exercises

The system shall support control exercises and shall verify submitted answers according to their defined evaluation model.

The results of control exercises shall contribute to the corresponding skill states and topic completion status.

### FR-11 Reflection

The system may collect the user's self-assessment of confidence or perceived difficulty.

Reflection data shall remain separate from objective learning outcomes and shall not independently determine skill state.

### FR-12 Learning State

For an authorized user, the system shall preserve the learning state required to resume learning.

The preserved state shall include, where applicable, topic statuses, skill states, exercise statistics, scheduled repetitions, and reflection results.

The system shall support resetting accumulated learning progress.

### FR-13 Guest Access

The system shall support learning activities without mandatory registration.

Persistent user-specific learning state shall be associated with an authorized user.

## 6. Quality Requirements

### QR-01 Correctness

The system shall not accept an invalid mathematical operation as a valid state transition.

### QR-02 State Integrity

The system shall not modify learning state using invalid or incomplete state-changing data.

### QR-03 Persistence

After a successfully completed state-changing operation, the resulting learning state shall be persisted automatically.

A successfully completed operation or exercise shall not be reported as saved unless its required state has been successfully persisted.

### QR-04 Recovery

After page reload or subsequent authorization, the system shall restore the last successfully saved learning state.

### QR-05 Security

Authorization and access control shall be enforced on the server.

Client-side state shall not be authoritative for learning outcomes, skill states, or topic statuses.

### QR-06 Compatibility

The system shall operate in the supported current versions of Google Chrome, Microsoft Edge, and Mozilla Firefox.

The system shall not depend on a specific client operating system when accessed through a supported browser.

### QR-07 Accessibility

The system shall support different methods of interaction available on the target device.

Information about status and errors shall not be communicated by color alone.

### QR-08 Localization
