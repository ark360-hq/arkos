# ADR-0049: Azure OpenAI adapter — client tenant, regional AU Standard

**Status:** Accepted

**Date:** 2026-10-10

**Supersedes:** none

**Superseded by:** none

---

## Context

I1 (issue #419) and later intelligence work need a chat-completions path. SPEC-0179 already pins
platform infrastructure to `australiaeast` / `australiasoutheast`. The 10 Oct 2026 tech-strategy
decision (issue #465) adds: client data sent to a model stays in an **Australian-region GPT
deployment** in the **customer's** Azure tenant — not a shared ARK360-owned model tenant, and not
public OpenAI.

Forces: Domain must stay dependency-free; `Azure.Identity` is pinned at 1.13.1 (ADR-0033), so a new
`Azure.AI.OpenAI` package that bumps `Azure.Core` is unwelcome; Azure OpenAI **Global Standard**
deployments process prompts in any Azure region even when the resource lives in Australia; this
repository must not provision the billable account.

## Decision

We will add a provider-neutral `ILanguageModelClient` in `BTROS.Domain` and one production adapter
that calls the Azure OpenAI Chat Completions REST API over `HttpClient` (no model SDK, no Semantic
Kernel, no `IChatClient`).

The binding is configuration-only (`AzureOpenAI:*` from Key Vault / env, ADR-0036). It is valid only
when all of the following hold:

1. **Client tenant.** `TenantMode` is `Client`. Shared, platform, or ARK360-owned tenants are rejected.
2. **Australian region.** `Region` is `australiaeast` or `australiasoutheast` (same pin as SPEC-0179).
3. **Regional Standard deployment.** `DeploymentType` is `Standard`. Global Standard, Data Zone, and
   provisioned-throughput types are rejected so processing geography matches the resource region.
4. **Azure OpenAI host.** `Endpoint` is `https://{resource}.openai.azure.com/` — not `api.openai.com`
   and not a non-Azure host.

Development and Testing use a deterministic stub. Outside those environments, a partial or illegal
binding fails startup. An absent binding does not fail startup (no caller exists yet) but
`CompleteAsync` fails closed and sends nothing.

This repository does not create the Azure OpenAI account. The customer provisions a regional
Standard GPT deployment in their subscription; keys go in that environment's Key Vault.

## Consequences

Plus: I1 and later features share one fail-closed seam; CI never calls a model; residency and
tenancy rules are enforced in code rather than in a runbook; no new NuGet package and no
`Azure.Identity` pin conflict.

Minus: region cannot be read from the `*.openai.azure.com` hostname, so `Region` is a configured
claim checked against an allowlist — a wrong-but-AU-shaped value would still need operational
review of the actual Azure resource. Live completion against a real deployment is not exercised by
CI and waits on the customer-provisioned resource plus a caller (issue #419).
