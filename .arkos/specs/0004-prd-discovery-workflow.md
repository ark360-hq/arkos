---
id: SPEC-0004
slug: prd-discovery-workflow
title: PRD discovery template and agent interview
status: Approved
owner: "@ark360-hq"
created: 2026-05-21
touches-personal-data: false
trust-boundaries-crossed: false
linked-adrs: []
linked-threat-model: null
---

# SPEC-0004: PRD discovery template and agent interview

## Problem

Developers adopting ARK OS often arrive with a short product idea in chat. Without a structured PRD and clarifying questions, agents scaffold the wrong shape, skip non-goals, and treat SPEC-0001 as generic template work instead of PRD-driven architecture. A repeatable PRD template and discovery prompt is needed before foundation specs and stack modules (v0.2).

## Out of scope

1. Implementing `npx create-arkos@latest` CLI ingestion of PRD files (deferred to v0.2).
2. Automating SPEC-0001 or stack scaffold generation from the PRD in this change.
3. CI enforcement that a PRD exists (manual and agent-enforced only for v0.1).
4. Localisation of the interview beyond Australian English.

## Requirements

### Ubiquitous

> REQ-001: The template repository shall ship `.arkos/prd/_template.md` with sections for product intent, users, MVP scope, non-goals, compliance, roadmap epics, and success metrics.

> REQ-002: The template repository shall ship `.arkos/prompts/prd-discovery.md` listing required clarification questions the agent must ask before writing the PRD.

### Event-driven

> REQ-003: WHEN a user describes a new product idea and `.arkos/prd/product.md` does not exist, the agent shall run the PRD discovery interview from `.arkos/prompts/prd-discovery.md` before drafting SPEC-0001.

> REQ-004: WHEN all required clarification questions are answered or explicitly deferred with rationale, the agent shall write `.arkos/prd/product.md` from `.arkos/prd/_template.md`.

### State-driven

> REQ-005: WHILE `.arkos/prd/product.md` has `status: Draft`, the agent shall not approve or implement SPEC-0001 foundation work.

### Unwanted behaviour

> REQ-006: IF the user provides only a vague idea THEN the agent shall not invent MVP scope, non-goals, or stack choices without asking clarifying questions first.

## Acceptance criteria

| Req | Test | Type |
|---|---|---|
| REQ-001 | `.arkos/prd/_template.md` exists with roadmap and non-goals sections | manual |
| REQ-002 | `.arkos/prompts/prd-discovery.md` lists at least ten required questions | manual |
| REQ-003 | `AGENTS.md` references PRD discovery before greenfield SPEC-0001 | manual |
| REQ-004 | `.arkos/prd/README.md` documents the interview-to-PRD flow | manual |
| REQ-005 | `AGENTS.md` states PRD must be Approved before SPEC-0001 | manual |
| REQ-006 | `prd-discovery.md` includes instruction not to guess unanswered fields | manual |

## Accessibility notes

| Criterion | WCAG reference | How addressed |
|---|---|---|
| PRD interview | N/A | Discovery is documentation-only; product UI a11y captured in PRD section for downstream specs. |
