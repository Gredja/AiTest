# AGENTS.md — Gredja

Instructions for AI agents working with the Gredja project.

## Rules

Project rules are also available as a skill: `/gredja-rules` (`.claude/skills/gredja-rules/SKILL.md`).

Detailed rule files are stored in `Rules/` — each rule set in a separate markdown file:
Refer to the corresponding rule file when working with an entity.

- `Rules/models.md` — model building rules (Model/Request, properties, naming)
- `Rules/comments.md` — when comments are needed in code
- `Rules/assertions.md` — FluentAssertions, key patterns
- `Rules/code.md` — general code writing rules (naming, types, file structure)
- `Rules/git.md` — remote, commits, secrets
- `Rules/workflow.md` — plan → approval → changes → report
- `Rules/config.md` — endpoints, configuration

File structure: see `FILE_STRUCTURE.md`.

## Knowledge Graph

Project knowledge graph: `graphify-out/graph.json`
Visualization: `graphify-out/model-dependencies.html`

Update the graph:
```bash
$env:PATH = "C:\Users\User\.local\bin;$env:PATH"
graphify . --code-only --force
```

## Session Start Hook (mandatory)

**Before doing ANYTHING else at the start of every session, read BOTH memory files:**
1. `C:\Users\User\.local\share\mimocode\memory\projects\global\MEMORY.md`
2. `C:\Users\User\.local\share\mimocode\memory\projects\gredja\MEMORY.md`

This is not optional. Even if the user's first message seems unrelated — read memory first, then respond.

## Memory Navigation

To access saved data:

- `memory({ operation: "search", query: "<keyword>" })` — search project memory
- `Read(file_path="<path>")` — read a specific file
- `task({ operation: "list" })` — list active tasks
- `actor({ operation: "status", actor_id: "<id>" })` — agent status

**Read project memory first:**
- `C:\Users\User\.local\share\mimocode\memory\projects\global\MEMORY.md` — global rules, conventions, architecture decisions
- `C:\Users\User\.local\share\mimocode\memory\projects\gredja\MEMORY.md` — Gredja-specific context, rules, roadmap, curriculum progress

## Memory Structure

```
~/.local/share/mimocode/memory/
├── projects/global/MEMORY.md    — rules, conventions, architecture decisions
├── projects/gredja/MEMORY.md    — project-specific context, rules, roadmap, curriculum progress
├── sessions/<sid>/checkpoint.md — current session state
└── sessions/<sid>/notes.md      — notes and observations
```

## Graphify Commands

```bash
graphify query "show all models"           # query the graph
graphify path "ProductModel" "RatingModel" # path between nodes
graphify explain "UserModel"               # explain a node
```
