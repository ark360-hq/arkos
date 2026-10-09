# ADR-0002: EARS as the Requirements Notation

**Status:** Accepted

**Date:** 2026-05-13

**Supersedes:** (none)

**Superseded by:** (none)

---

## Context

AI coding agents need unambiguous, testable requirements. Free-form prose requirements produce inconsistent implementations across sessions: agents fill in ambiguity differently each time, and there is no systematic way to verify that all requirements have been addressed. A structured notation is required that produces requirements that are independently verifiable and that map directly to test cases.

## Decision

EARS (Easy Approach to Requirements Syntax), developed by Mavin and Stephenson (2010), is the required notation for all specs in ARK OS-governed repositories. EARS provides five sentence patterns: Ubiquitous, Event-driven, State-driven, Optional feature, and Unwanted behaviour. Every requirement must use one of these patterns and must receive a unique REQ-NNN identifier that maps to at least one test in the acceptance criteria table.

## Consequences

Plus: Each requirement is independently testable and maps directly to a Gherkin scenario or test case. The acceptance criteria table provides a machine-checkable trace.

Plus: EARS is native in AWS Kiro IDE and aligns with emerging industry standards for agentic development.

Minus: Requires a brief learning curve for developers unfamiliar with EARS. The five patterns cover the most common cases but may require creative application for complex or cross-cutting requirements.
