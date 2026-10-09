# Standards Reference

ARK OS v0.2.1 binds to these standards. Each entry lists the standard, its specific version, the rationale for inclusion, and the authoritative source.

---

## Agent instructions: AGENTS.md

**Version:** Current (unversioned as of May 2026)
**Rationale:** The only cross-vendor agent instruction convention with majority adoption. Native in GitHub Copilot, Cursor, Codex, Windsurf, Gemini CLI, and Amp. Stewarded by the Linux Foundation's Agentic AI Foundation (AAIF) since December 2025.
**Source:** https://agents.md

---

## Tool integration: Model Context Protocol (MCP)

**Version:** Current
**Rationale:** AAIF-stewarded. Native across Claude, Cursor, Copilot, ChatGPT, and Gemini. The emerging standard for tool-use integration between agents and services.
**Source:** https://modelcontextprotocol.io

---

## Requirements notation: EARS

**Version:** Current (Mavin and Stephenson, 2010)
**Rationale:** Five sentence patterns (Ubiquitous, Event-driven, State-driven, Optional, Unwanted) that produce independently testable requirements. Each pattern maps directly to a test case. Native in AWS Kiro IDE.
**Source:** "Easy Approach to Requirements Syntax" - Mavin, Wilkinson, Tuffin and Rabel

---

## Architectural decisions: ADR (Nygard format)

**Version:** Current (Michael Nygard, 2011)
**Rationale:** Battle-tested since 2011. Lightweight markdown format. Immutable record of context, decision, and consequences. Widely adopted across enterprise software teams.
**Source:** https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions

---

## Application security: OWASP ASVS

**Version:** v5.0.0 Level 1 (released May 2025)
**Rationale:** L1 is the first layer of defence, covering the most common and impactful vulnerability classes. The floor for any software handling user data. Required by most enterprise procurement questionnaires.
**Source:** https://owasp.org/www-project-application-security-verification-standard/

---

## Cyber hygiene: ASD Essential Eight

**Version:** Maturity Level 1
**Rationale:** The Australian Signals Directorate's published minimum for cyber security. The procurement floor for Australian government and enterprise contracts. ML1 is achievable by a solo developer.
**Source:** https://www.cyber.gov.au/resources-business-and-government/essential-cyber-security/essential-eight

---

## Privacy: Privacy Act 1988 (Cth) - Australian Privacy Principles

**Version:** APP 1, 5, 11
**Rationale:** APP 1 (open and transparent management of personal information), APP 5 (notification of collection), APP 11 (security of personal information). The minimum obligations for any Australian software that handles personal data.
**Source:** https://www.oaic.gov.au/privacy/australian-privacy-principles

---

## Accessibility: WCAG

**Version:** 2.2 AA (also ISO/IEC 40500:2025, approved October 2025)
**Rationale:** The international standard for digital accessibility. Required by Australian government procurement. AA is the level that balances coverage with practical implementability.
**Source:** https://www.w3.org/TR/WCAG22/

---

## Software Bill of Materials: CycloneDX

**Version:** 1.6+
**Rationale:** OWASP project. Lighter than SPDX. Better tooling support for typical web stacks. Machine-readable format for supply chain transparency. Required by an increasing number of enterprise procurement processes.
**Source:** https://cyclonedx.org

---

## Commit conventions: Conventional Commits

**Version:** 1.0.0
**Rationale:** Machine-readable commit history. Enables automated CHANGELOG generation and semantic version calculation. Widely adopted; supported by most CI tooling.
**Source:** https://www.conventionalcommits.org/en/v1.0.0/

---

## Changelog format: Keep a Changelog

**Version:** 1.1.0
**Rationale:** Human-readable changelog convention. Pairs with Conventional Commits for automated generation. Clear structure (Added, Changed, Deprecated, Removed, Fixed, Security) that procurement reviewers can navigate.
**Source:** https://keepachangelog.com/en/1.1.0/

---

## Out of scope for v0.2.1

The following standards are explicitly out of scope for ARK OS v0.2.1. They are available in later versions or via a separately scoped ARK360 engagement.

| Standard | Why excluded |
|---|---|
| IRAP (Information Security Registered Assessors Program) | Requires formal assessor engagement; not achievable by solo developers |
| Essential Eight ML2+ | Requires organisational controls beyond the codebase |
| SLSA L3/L4 | Build provenance requirements exceed solo developer tooling |
| ISO 27001 full ISMS | Requires an organisational information security management system |
| SOC 2 Type II | Requires sustained operational evidence collection over months |
| APRA CPS 234 | Applies to APRA-regulated entities only |
