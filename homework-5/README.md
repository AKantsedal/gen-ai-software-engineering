# Homework 5 — Configure MCP Servers

**Student:** Artur Kantsedal
**Task:** Install and configure three external MCP servers (GitHub, Filesystem, Jira/Notion) and build one custom MCP server with FastMCP.

---

## Overview

This homework connects Claude Code to external tools and services via the Model Context Protocol (MCP). Four servers are configured and demonstrated:

| Task | Server | Purpose |
|------|--------|---------|
| 1 | GitHub MCP | List PRs, commits, issues from GitHub repos |
| 2 | Filesystem MCP | Read and explore local directories |
| 3 | Notion MCP | Query Notion pages and databases |
| 4 | Custom FastMCP | Custom server exposing a `read` tool backed by `lorem-ipsum.md` |

All servers are registered in `.mcp.json` at the repo root.

---

## Project Structure

```
homework-5/
├── README.md
├── HOWTORUN.md
├── github-mcp/                        ← screenshots and notes for Task 1
├── custom-mcp-server/
│   ├── server.py                      ← custom FastMCP server
│   ├── lorem-ipsum.md                 ← source text for the read tool
│   └── requirements.txt               ← includes fastmcp
└── docs/
    └── screenshots/
        ├── github-mcp-result.png
        ├── filesystem-mcp-result.png
        ├── notion-mcp-result.png
        └── custom-mcp-read-tool-result.png
```

---

## MCP Servers

### Task 1 — GitHub MCP

Uses the official `@modelcontextprotocol/server-github` package via `npx`. Authenticated with a GitHub Personal Access Token. Demonstrated by listing all pull requests in this repository.

### Task 2 — Filesystem MCP

Uses `@modelcontextprotocol/server-filesystem` via `npx`. Pointed at the homework directory to list and read local files.

### Task 3 — Notion MCP

Uses the Notion MCP server authenticated with a Notion integration token. Demonstrated by querying the last 5 bug-related pages from a Notion project database.

### Task 4 — Custom FastMCP Server

A custom Python MCP server built with `fastmcp`. Exposes:
- **Resource** `lorem://text/{word_count}` — returns the first `word_count` words from `lorem-ipsum.md`
- **Tool** `read` — callable by Claude; accepts an optional `word_count` parameter and returns the resource content

See `custom-mcp-server/server.py` and `HOWTORUN.md` for full details.

---

## How I Used AI

- Claude Code (Sonnet 4.6 with 1M context) was used throughout this session to configure MCP servers, generate the custom FastMCP server, and produce all documentation.
- GitHub MCP was tested live within this Claude Code session — the `list_pull_requests` call returned real PR data from this repository.
- The custom server was designed to satisfy the exact resource/tool contract specified in the task, then verified via MCP connection.

---

## Prerequisites

See [HOWTORUN.md](HOWTORUN.md) for full setup and run instructions.

- Node.js (for GitHub and Filesystem MCP servers via `npx`)
- Python 3.10+ (for the custom FastMCP server)
- `fastmcp` Python package
- Claude Code CLI (`claude`) with a valid Anthropic API key
- GitHub Personal Access Token (for GitHub MCP)
- Notion integration token (for Notion MCP)
