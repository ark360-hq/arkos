# Contributing to ARK OS

ARK OS welcomes contributions from the community. This document explains how to contribute to the framework itself.

If you are using ARK OS in your own project, this file is for you only if you want to propose changes to the framework.

---

## How to propose changes to the framework

Every change that edits a file in this repository needs an approved spec and a feature branch. There are no exceptions for documentation updates, config tweaks, typo fixes, CHANGELOG entries, or other small changes.

1. Write a spec in `.arkos/specs/` from `_template.md` using EARS notation. Include the problem, at least three out-of-scope items, requirements, and acceptance criteria.
2. If the change affects an architectural decision (which standard to bind to, which file format to use), file an ADR in `.arkos/adr/` before the spec is approved.
3. Set the spec `status` to `Approved` only after an explicit confirmation.
4. Open a GitHub issue that references the spec ID and summarises the change. The issue is the branch's traceable anchor (issue before branch).
5. Create a feature branch named `feat/SPEC-NNNN-slug`. Do not edit files on `main` or on a previously merged branch.
6. Implement the change, add tests mapped to each REQ, and update `CHANGELOG.md` under `[Unreleased]`.
7. Open a pull request that references `SPEC-NNNN` and includes `Closes #N` for the issue opened in step 4.

The spec must reach `status: Approved` before implementation starts.

---

## The spec-before-PR rule

No pull request may change a file without an approved spec. This applies to all contributors, including maintainers.

The build gate CI check enforces a spec reference: every PR must reference a `SPEC-[0-9]+` in the title, body, or commit messages.

---

## ADR requirements for structural changes

A structural change is any change that:

- Adds, removes, or renames a mandatory file in the ARK OS file tree
- Changes which standards ARK OS binds to
- Changes the gate schema in `.arkos/gates/`
- Changes the AGENTS.md template structure

Structural changes require an ADR filed in `.arkos/adr/` before the spec can be approved.

ADRs are immutable. Once accepted, an ADR cannot be edited. To change a decision, file a new ADR that supersedes the old one.

---

## Code of conduct

ARK OS follows the Contributor Covenant v2.1. All contributors are expected to:

- Be respectful in all communication.
- Critique ideas, not people.
- Accept that decisions are made through the spec and ADR process, not by argument volume.

Report conduct concerns to the ARK360 team via `SECURITY.md`.

---

## Commit conventions

All commits must follow Conventional Commits 1.0:

```
<type>(<scope>): <description> (<spec-reference>)
```

Types: `feat`, `fix`, `docs`, `chore`, `refactor`, `test`.

Example: `docs(quickstart): clarify IDE adapter wiring (SPEC-0003)`

---

## Licence

By contributing, you agree that your contributions are licenced under Apache 2.0. Copyright is retained by the contributor; attribution to ARK360 is required as per the licence terms.
