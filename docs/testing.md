# Testing

## 1. Purpose

Testing verifies that the implemented system meets its defined behavior and does not introduce regressions.

## 2. Test Types

The project uses:
- **Unit tests** — isolated testing of application logic and mathematical operations.
- **Integration tests** — testing interaction between application components and the database.
- **API tests** — testing HTTP endpoints, request validation, responses, and error handling.
- **UI tests** — testing critical user flows in the web interface when required.

## 3. Unit Testing

Unit tests cover:
- business rules;
- validation;
- mathematical operations;
- mathematical operation preconditions;
- calculation and evaluation logic;
- state transitions.

Unit tests do not depend on external services or the real database.

## 4. Integration Testing

Integration tests cover:
- database operations;
- persistence and retrieval;
- interaction between application components;
- transaction-dependent behavior;
- data consistency.

Tests use an isolated test database or test database environment.

## 5. API Testing

API tests cover:
- valid requests;
- invalid requests;
- validation errors;
- authorization and access restrictions;
- successful responses;
- expected error responses;
- persistence of changes through API operations.

## 6. Mathematical Logic Testing

Each supported mathematical operation has tests for:
- valid input;
- invalid input;
- boundary cases;
- resulting mathematical state;
- preservation of mathematical correctness.

An invalid operation must not produce an accepted mathematical state.

## 7. Test Naming

Test names describe the tested behavior and expected result.

The naming format follows:

`MethodOrScenario_Condition_ExpectedResult`

## 8. Test Isolation

Tests are independent and do not rely on execution order or state left by other tests. Test data is created and cleaned up within the test scope where required.

## 9. Regression Testing

A change that affects existing behavior requires corresponding tests or updates to existing tests. All relevant automated tests pass before the change is accepted.

## 10. Acceptance Criteria

Testing is considered complete when:
- all required automated tests pass;
- no critical or blocking defects remain;
- core user flows operate correctly;
- mathematical operations produce expected results;
- data is correctly persisted and retrieved.