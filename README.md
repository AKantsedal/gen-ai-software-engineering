# Artur Kantsedal — GenAI & Agentic AI for Software Engineering

Senior .NET engineer (10 years), working AI-first with Claude Code. This repository holds my work from the
*GenAI and Agentic AI for Software Engineering* course (2026): multi-agent pipelines, spec-driven development,
skills, hooks and MCP, applied to small FinTech-style .NET services.

[LinkedIn](https://www.linkedin.com/in/akantsedal/)

## Highlights

| | What | Key result |
|---|---|---|
| [HW4](homework-4/) | Multi-agent bug-fix pipeline on a .NET API: research → verify → plan → fix → security review → unit tests, run by one script | Found and fixed two seeded logic bugs and a hardcoded auth bypass; the security agent then flagged that the API had no authentication at all. Model chosen per agent by risk and cost |
| [HW6](homework-6/) | Spec-driven multi-agent transaction pipeline in .NET 10: validation → fraud scoring → settlement | 53 xUnit tests, ~94% coverage; a Claude Code hook blocks `git push` below 80%; custom FastMCP server to query results |
| [HW3](homework-3/) | Layered, agent-executable specification for a card (BIN) lookup service | Agent rules enforce PAN masking, input validation and a read-only service boundary |

## How I work with agents — where to look

- **Model per task.** Opus where a miss is expensive (verification, security), Sonnet for implementation,
  Haiku for mechanical work (tests). See `homework-4/agents/`.
- **Verified output, not trusted output.** The verifier grades research Gold / Silver / Bronze by checking every
  `file:line` claim against the source. See `homework-4/skills/research-quality-measurement.md`.
- **Guardrails as code.** Coverage gate as a PreToolUse hook (`homework-6/.claude/settings.json`); PII masking and
  structured audit logging required by the spec (`homework-6/task-1-specification/`).
- **Agents connected to real systems.** GitHub, Filesystem and Notion MCP servers; a custom Python FastMCP server
  with tools and a resource over the pipeline results (`homework-6/mcp/server.py`).

## All assignments

| # | Topic | Status |
|---|---|---|
| [1](homework-1/) | Banking transactions API built with Claude Code (ASP.NET Core 8): validation, filtering, account summary | Done |
| [2](homework-2/) | Customer support ticket system | Not done |
| [3](homework-3/) | Specification-driven design (documents only, by design) | Done |
| [4](homework-4/) | Multi-agent bug-fix pipeline | Done |
| [5](homework-5/) | MCP servers: GitHub, Filesystem, Notion, custom | Done |
| [6](homework-6/) | Capstone: multi-agent pipeline with skills, hooks, MCP, tests | Done |

Each homework folder has its own `README.md` and `HOWTORUN.md`.

**Stack:** Claude Code (sub-agents, skills, slash commands, hooks), MCP, FastMCP, C# / .NET 8–10, xUnit, Python.

---

Original course instructions: [COURSE.md](COURSE.md)
