# PRD discovery interview (agent playbook)

Use this playbook when the developer describes a **new product idea** and `.arkos/prd/product.md` does not exist or is incomplete.

**Goal:** Produce an **Approved** `.arkos/prd/product.md` that is specific enough to draft SPEC-0001 (foundation: architecture + scaffold) and a roadmap of SPEC-0002+ epics.

**Hard rules:**

- Do **not** invent MVP scope, users, stack, or non-goals to fill gaps.
- Do **not** write application code, scaffold a stack, or approve SPEC-0001 until the PRD is **Approved**.
- Ask clarifying questions in plain language. Use the IDE's structured question UI when available (2 to 4 questions per turn).
- Record deferrals in the PRD **Open questions** table with owner and target date.

---

## Phase 1 - Capture the idea

Repeat back the developer's idea in one paragraph. Confirm:

> "I'll turn this into a PRD in `.arkos/prd/product.md` and ask a short set of required questions. After you approve the PRD, we'll draft SPEC-0001 for architecture and scaffold. Feature work starts at SPEC-0002. Is that the process you want?"

Store the original wording in PRD frontmatter `source_prompt`.

---

## Phase 2 - Required clarification questions

Ask every question below. Skip only if already answered clearly in the idea; note "Confirmed from prompt" in the PRD.

### Product identity

| ID | Question | PRD section |
|---|---|---|
| Q1 | What is the **product name** and **repository slug** (lowercase, hyphenated)? | frontmatter, title |
| Q2 | In **one sentence**, what value does this deliver and to whom? | One-line value proposition |

### Users and problem

| ID | Question | PRD section |
|---|---|---|
| Q3 | Who are the **primary users** (roles, not demographics)? What job are they hiring the product to do? | Users and jobs to be done |
| Q4 | What is broken, slow, risky, or expensive about how they work **today**? | Problem |

### MVP scope

| ID | Question | PRD section |
|---|---|---|
| Q5 | What is the **single MVP outcome** we can ship first (ideally one measurable result)? | MVP outcome |
| Q6 | List **three to seven MVP capabilities** in priority order. Which one is absolutely first? | MVP capabilities |
| Q7 | Name **at least three non-goals** (things we will not build in MVP or v1). | Non-goals |

### Technical direction

| ID | Question | PRD section |
|---|---|---|
| Q8 | Is there a **required or preferred stack** (e.g. .NET, Node, mobile native)? If unknown, should the agent recommend options? | Stack and technical constraints |
| Q9 | What **UI surfaces** exist in MVP (web admin, API only, mobile, integrations)? | Stack and technical constraints |
| Q10 | Any **hosting or deployment** constraints (Azure, on-prem, air-gapped, SaaS multi-tenant)? | Stack and technical constraints |

### Security, privacy, compliance

| ID | Question | PRD section |
|---|---|---|
| Q11 | Will the product collect, store, or transmit **personal information**? What kinds? | Privacy and data |
| Q12 | What **trust boundaries** are crossed (login, third-party APIs, devices on customer networks, operator actions)? | Trust boundaries |
| Q13 | Which surfaces must meet **WCAG 2.2 AA** in MVP? | Accessibility |

### Success and roadmap

| ID | Question | PRD section |
|---|---|---|
| Q14 | How will we know MVP succeeded? Name **one to three metrics**. | Success metrics |
| Q15 | After MVP, what are the **next three to five epics** (titles only)? | Roadmap after MVP |

### Optional (ask if relevant)

| ID | Question | When to ask |
|---|---|---|
| Q16 | Key **integrations or vendors** (PMS, access control, identity)? | B2B / integration products |
| Q17 | **Regulatory** context beyond ARK OS baselines (sector, jurisdiction)? | Regulated domains |
| Q18 | **Team skills** that affect stack choice? | Small teams choosing stack |

---

## Phase 3 - Synthesis checks

Before writing the PRD, verify:

- [ ] At least **three non-goals** are explicit (not "TBD").
- [ ] MVP capabilities are **prioritised** (P0 vs P1).
- [ ] Each P0 capability has a proposed **SPEC-0002+** ID in the table (reserve SPEC-0001 for foundation).
- [ ] Personal data and trust boundaries are classified (update frontmatter booleans).
- [ ] Open questions table lists anything deferred, with owner.

If checks fail, return to Phase 2 for the missing answers.

---

## Phase 4 - Write the PRD

1. Copy [`.arkos/prd/_template.md`](../prd/_template.md) to `.arkos/prd/product.md` if `product.md` does not exist.
2. Fill every section from interview answers. Use Australian English. No em dashes.
3. Set `created` to today's date (YYYY-MM-DD).
4. Leave `status: Draft` until the developer explicitly approves.

Present a short summary: MVP outcome, P0 capabilities, top non-goals, proposed stack, and planned SPEC-0002 titles.

Ask:

> "Please review `.arkos/prd/product.md`. Reply **approve PRD** to set status to Approved, or list corrections."

Only after **approve PRD**, set `status: Approved` in frontmatter and proceed to SPEC-0001 foundation drafting (see `AGENTS.md`).

---

## Phase 5 - Handoff to SPEC-0001

After PRD approval, draft SPEC-0001 with:

- **Problem** derived from PRD Problem + MVP outcome
- **Out of scope** copied from PRD non-goals
- **Requirements** for architecture, scaffold, CI harness, and docs (not feature behaviour)
- **Acceptance criteria** that prove the repo is ready for SPEC-0002 implementation
- A table mapping PRD MVP capabilities to future spec IDs

Do not implement SPEC-0001 until the developer approves that spec separately.

---

## Pushback scripts

**User wants to skip the PRD:**

> "The PRD is what keeps SPEC-0001 and the scaffold aligned with your idea. I can run a focused interview (about ten questions) and write `product.md` in a few minutes. Shall we do that first?"

**User wants code immediately:**

> "I'll draft the PRD and SPEC-0001 for your review first. Once both are Approved, scaffold and CI wiring are the first implementation PR."

**User gives contradictory answers:**

> "I see a conflict between [A] and [B]. Which should the PRD treat as authoritative for MVP?"
