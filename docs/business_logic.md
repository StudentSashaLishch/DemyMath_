# DemyMath Business Logic

This document defines the core business rules of DemyMath. Unspecified implementation details remain open for future decisions.

## 1. Roles and Access

- **Guest:** can use learning features without registration. Progress persistence between sessions is not guaranteed.
- **User:** can access a personal profile, save learning progress, and restore it across sessions.
- **Administrator:** manages learning content and system settings.
- Public self-registration does not grant administrator privileges.

## 2. Learning Content

- Topics contain theory and practical exercises.
- Content supports multiple languages.
- Topics may define `Required Skills`, `Developed Skills`, and `Prerequisites`.
- Prerequisites inform the user about recommended prior learning but do not block topic access.
- Learning content is stored separately from application code and interface resources.

## 3. Topic Status

Each topic has one of the following statuses:

- **New:** the topic has not been studied.
- **In Progress:** theory has been reviewed, but the required practice is incomplete.
- **Mastered:** the required assessment tasks have been completed and the associated skills have reached the required level.
- **Needs Review:** the topic or its associated skills are due for review.

Topic status is distinct from the state of any individual skill.

## 4. Exercises and Assessment

- The initial version supports single-choice and numeric-answer exercises.
- The server determines answer correctness.
- Exercises may target individual skills or combinations of skills.
- Assessment results contribute to skill-state updates and topic mastery.
- Hint usage is recorded separately.

## 5. Skills and Adaptive Learning

- Skill state is tracked independently of topic status.
- One exercise may contribute to multiple skills.
- Skill-state updates consider correctness, errors, attempts, hint usage, and assessment results.
- Exercise selection adapts to skill state, previous results, and review needs.

## 6. Spaced Repetition and Reflection

- Review scheduling considers skill states, exercise results, errors, hint usage, and previous reviews.
- Successful performance generally increases review intervals; difficulties shorten them.
- Self-assessed confidence is stored separately from objective learning results and does not independently determine skill state.

## 7. Learning State

For authenticated users, the system stores sufficient information to restore learning progress, including:

- Topic statuses and skill states.
- Exercise-attempt statistics and assessment results.
- Hint usage.
- Scheduled reviews.
- Reflection results.

Users can reset their accumulated learning progress.