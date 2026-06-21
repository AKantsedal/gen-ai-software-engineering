# How to Run — Homework 5

## Prerequisites

| Requirement | Check |
|-------------|-------|
| **Node.js** | `node --version` |
| **Python 3.10+** | `python3 --version` |
| **Claude Code CLI** | `claude --version` |
| **GitHub PAT** | Token with `repo`, `read:org`, `read:user` scopes |
| **Notion integration token** | From Notion → Settings → Integrations |

---

## 1. MCP Configuration

All servers are registered in `.mcp.json` at the repo root. Claude Code picks this up automatically when launched from this directory.

To verify all servers are loaded:

```bash
claude mcp list
```

---

## 2. GitHub MCP

No installation needed — runs via `npx` on first use.

Add to Claude Code with your GitHub PAT:

```bash
claude mcp add github-mcp -s project \
  -e GITHUB_PERSONAL_ACCESS_TOKEN=your-token-here \
  -- npx -y @modelcontextprotocol/server-github
```

**Test interaction:**
> "List the recent pull requests in AKantsedal/gen-ai-software-engineering"

---

## 3. Filesystem MCP

No installation needed — runs via `npx` on first use.

Add to Claude Code with a path to expose:

```bash
claude mcp add filesystem-mcp -s project \
  -- npx -y @modelcontextprotocol/server-filesystem /path/to/directory
```

**Test interaction:**
> "List the files in the homework-5 directory"

---

## 4. Notion MCP

Add to Claude Code with your Notion integration token:

```bash
claude mcp add notion-mcp -s project \
  -e NOTION_API_TOKEN=your-notion-token-here \
  -- npx -y @notionhq/notion-mcp-server
```

**Test interaction:**
> "Give me the tickets/pages of the last 5 bugs on a project"

---

## 5. Custom FastMCP Server

### Install dependencies

```bash
cd homework-5/custom-mcp-server
pip install -r requirements.txt
```

### Run the server

```bash
python server.py
```

The server starts and listens for MCP connections via stdio.

### Connect to Claude Code

Add to `.mcp.json` or run:

```bash
claude mcp add custom-lorem -s project \
  -- python /path/to/homework-5/custom-mcp-server/server.py
```

### Use the `read` tool

Once connected, ask Claude:
> "Use the read tool to get 50 words from the lorem ipsum text"

Or with default word count (30):
> "Use the read tool"

### Resource URI (direct access)

The resource is also accessible as a URI:

```
lorem://text?word_count=30
```

Claude can read this directly or via the `read` tool.

---

## MCP Config Reference

Full `.mcp.json` at repo root:

```json
{
  "mcpServers": {
    "github-mcp": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-github"],
      "env": {
        "GITHUB_PERSONAL_ACCESS_TOKEN": "<your-token>"
      }
    },
    "filesystem-mcp": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-filesystem", "/path/to/dir"]
    },
    "notion-mcp": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@notionhq/notion-mcp-server"],
      "env": {
        "NOTION_API_TOKEN": "<your-token>"
      }
    },
    "custom-lorem": {
      "type": "stdio",
      "command": "python",
      "args": ["/path/to/homework-5/custom-mcp-server/server.py"]
    }
  }
}
```

> Note: `.mcp.json` is excluded from git (`.gitignore`) because it contains secret tokens.
