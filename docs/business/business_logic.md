# DemyMath Domain

## Contents

1. Purpose
2. Entities
3. Value Objects
4. Mathematical Domain
5. Domain Policies
6. Domain Services
7. Domain Events
8. Domain Boundary

## 1. Purpose

DemyMath covers the domain of interactive mathematics learning.

The domain includes:
- learning content;
- mathematical learning activities;
- interactive mathematical objects and transformations;
- learning attempts and results;
- self-assessment and progress;
- user comments and content moderation;
- administration of educational content.

## 2. Entities

|Entity|Basic Fields|Description|Invariants|
|---|---|---|---|
|`User`|`Id`, `Name`, `Role`|System user participating in learning or administration.|`Id` is immutable and unique. `Role` is one of the defined roles.|
|`Topic`|`Id`, `Title`, `Description`|Mathematical topic containing related educational content and activities.|`Id` is immutable and unique. `Title` is required.|
|`LearningMaterial`|`Id`, `TopicId`, `Title`, `Content`|Educational material explaining a mathematical concept or procedure.|Belongs to one topic. Required content is present.|
|`Task`|`Id`, `TopicId`, `Statement`, `Type`|Practical mathematical activity performed by a student.|Belongs to one topic. Defines the information required for evaluation.|
|`Test`|`Id`, `TopicId`, `Title`|Assessment consisting of questions.|Belongs to one topic. Contains its defined questions.|
|`Question`|`Id`, `TestId`, `Content`, `Type`|Individual test item.|Belongs to one test. Has a defined answer/evaluation model.|
|`Attempt`|`Id`, `UserId`, `ActivityId`, `Status`, `Result`|A student's execution of a learning activity.|Belongs to one user and one activity. A completed attempt has a final result.|
|`SelfAssessment`|`Id`, `UserId`, `TopicId`, `Level`|Student's declared understanding level for a topic.|Belongs to one user and one topic. `Level` is a defined value.|
|`LearningProgress`|`UserId`, `TopicId`, `State`|Student's learning state for a topic.|Identifies one user-topic pair. `State` is a defined value.|
|`Comment`|`Id`, `UserId`, `Content`, `TargetId`, `Status`|Student comment associated with learning content.|Belongs to one user and one supported target. `Status` is a defined moderation state.|

### Roles

`User.Role` has two initial values:

|Role|Responsibility|
|---|---|
|`Student`|Uses learning materials and activities, submits results, self-assessments, and comments.|
|`Administrator`|Manages educational content and moderates student comments.|

Administrative permissions do not change the ownership of student-generated data.

## 3. Value Objects

|Value Object|Description|Invariants|
|---|---|---|
|`UserId`|Identifies a user.|Immutable and uniquely identifies one user.|
|`TopicId`|Identifies a topic.|Immutable and uniquely identifies one topic.|
|`LearningMaterialId`|Identifies learning material.|Immutable and unique.|
|`TaskId`|Identifies a task.|Immutable and unique.|
|`TestId`|Identifies a test.|Immutable and unique.|
|`QuestionId`|Identifies a question.|Immutable and unique.|
|`AttemptId`|Identifies an attempt.|Immutable and unique.|
|`CommentId`|Identifies a comment.|Immutable and unique.|
|`SelfAssessmentLevel`|Represents the student's selected understanding level.|Contains only defined assessment values.|
|`LearningProgressState`|Represents the learning state for a topic.|Contains only defined progress values.|
|`AttemptStatus`|Represents the lifecycle state of an attempt.|Contains only defined attempt states.|
|`CommentStatus`|Represents the moderation state of a comment.|Contains only defined moderation states.|

## 4. Mathematical Domain

### Mathematical Object

`MathematicalObject` represents a mathematical structure manipulated by the student.

The initial supported types are:
- algebraic expression;
- equation.

Additional mathematical object types may be introduced without changing the rules of existing object types.

### Mathematical Operation

`MathematicalOperation` represents a transformation applied to a mathematical object.

Each operation has:
- an applicable mathematical object state;
- defined preconditions;
- a resulting state.

An operation is applied only when its preconditions are satisfied. An invalid operation does not create a new valid state.

Mathematical correctness is part of the domain and is independent of the user interface.

## 5. Domain Policies

|Domain Policy|Description|
|---|---|
|Mathematical transformation policy|Determines whether a mathematical operation is valid for the current mathematical object state.|
|Attempt completion policy|Determines when an attempt can transition to a completed state and receive a final result.|
|Progress policy|Determines how a recorded learning result affects the learning state associated with a topic.|
|Comment moderation policy|Determines which moderation states are available for student comments and which users may apply them.|
|Content management policy|Determines which users may create, modify, publish, or make educational content unavailable.|

Specific thresholds, state transitions, and evaluation rules are defined when the corresponding functionality is specified.

## 6. Domain Services

No domain service is fixed at the current stage.

A domain service is introduced only when a domain operation cannot be naturally owned by one entity or value object.

## 7. Domain Events

No domain events are currently defined.

Events are introduced only when a significant domain state change must be consumed independently by another part of the system.

## 8. Domain Boundary

The domain owns:
- entities and their invariants;
- value objects;
- mathematical validity rules;
- learning activity rules;
- attempt and result rules;
- self-assessment and progress rules;
- content management rules;
- comment moderation rules.

The domain does not define:
- HTTP endpoints and transport;
- MVC controllers and views;
- client-side presentation;
- database schema or ORM configuration;
- authentication implementation;
- deployment and infrastructure configuration.