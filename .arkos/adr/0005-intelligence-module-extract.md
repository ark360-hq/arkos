# ADR-0005: Extract the Intelligence module from btros

**Status:** Accepted

**Date:** 2026-10-10

**Supersedes:** (none)

**Superseded by:** (none)

---

## Context

ARK360 products need a shared way to call Azure OpenAI and Azure AI Search with a governed gateway and an audit trail. James Park approved that reuse on 10 October 2026 (issue #18). The first working adapter and RAG slice are being built in btros, not in this template.

Forking btros into ARK OS would create two trees. Inventing a second stack here would do the same. The template must wait for those btros sources and then extract them.

## Decision

This project will add a reusable Intelligence module to ARK OS by extracting Azure OpenAI Bicep, Azure AI Search Bicep, and the governed gateway contract from the btros Azure OpenAI adapter and RAG slice after those sources land. The module will not be a fork. Implementation code will not land before those sources land. No billable Azure, Entra, or Stripe resources will be created from this repository.

## Consequences

Plus: Other ARK OS products reuse one gateway contract and one infrastructure template.

Plus: The audit-call obligation is defined before implementation.

Plus: Extraction keeps btros as the source of truth until the module exists here.

Minus: The module cannot ship until btros lands the adapter and RAG slice.

Minus: This token cannot see `ark360-hq/btros`, so the extract step needs a later run with access to that repository.
