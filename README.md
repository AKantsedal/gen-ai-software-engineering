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

<<<<<<< Updated upstream
## 🚀 Getting Started

### Step 1: Fork This Repository

1. Click the **Fork** button in the top-right corner of this repository page
2. Select your personal GitHub account as the destination
3. Wait for GitHub to create your personal copy of the repository

### Step 2: Clone Your Fork

```bash
git clone https://github.com/YOUR_USERNAME/ai-assisted-dev-homework.git
```

---

## 📁 Repository Structure

```
ai-assisted-dev-homework/
├── 📄 README.md                    # This file
├── 📂 homework-1/                  # Homework 1: Simple API with AI Assistance
│   ├── 📄 README.md               # Your documentation for HW1
│   ├── 📂 src/                    # Your source code
│   ├── 📂 docs/                   # Additional documentation
│   │   └── 📂 screenshots/        # Screenshots demonstrating AI usage
│   └── 📂 demo/                   # Demo files and run scripts
├── 📂 homework-2/                  # Homework 2: Enhanced App with Tests
├── 📂 homework-3/                  # Homework 3: App from Specification
├── 📂 homework-4/                  # Homework 4: Multi-Agent System
├── 📂 homework-5/                  # Homework 5: MCP Server Configuration
└── 📂 homework-6/                  # Homework 6: Capstone Project
```

---

## 📤 How to Submit Your Homework

### 1️⃣ Create a Branch for Each Assignment

```bash
# For homework 1
git checkout -b homework-1-submission

# Work on your assignment...

git add .
git commit -m "Complete homework 1"
git push origin homework-1-submission
```

### 2️⃣ Create a Pull Request

> **Pull request quality:** Do not submit a bare or one-line PR. Prepare a **detailed pull request** with a full description of what you implemented, how you used AI, how reviewers can run and verify your work, and 3-5 **screenshots** (or other demos) where the homework asks for evidence. Homework submitted **without a proper PR description** and the expected visual documentation **will be rejected**. Treat the PR body as the primary submission narrative—link to `README.md` / `HOWTORUN.md` in the repo, but the PR itself must still stand on its own.

1. Go to your forked repository on GitHub. *Do not create pull requests into original repository*
2. Click **"Compare & pull request"** or go to **Pull requests** → **New pull request**
3. Set the base repository to the **your personal repository**
4. Set the base branch to `main`
5. Set the compare branch to your `homework-X-submission` branch
6. Fill in the PR template with a **thorough** write-up, including:
   - ✅ Summary of what you implemented (enough detail for someone unfamiliar with your branch)
   - 🛠️ AI tools used (prompts, workflow, what you verified yourself)
   - ⚠️ Challenges encountered and how you addressed them
   - 📸 **Screenshots** showing the running solution and, where required, AI-assisted work—embed key images in the PR.  Also add them to `docs/screenshots/`

📌 Home Work without detailed description and screenshots in PR will not be accepted. 

### 3️⃣ Assign the Instructor for Review

1. In the Pull Request, click **"Reviewers"** on the right sidebar
2. Search for and add the instructor's GitHub username Alexey-Popov
3. Optionally add labels like `homework-1`, `ready-for-review`

---

## 📋 Submission Requirements

Each homework submission **MUST** include a merge-ready **pull request** on your fork that matches the expectations under **Create a Pull Request** (detailed description, how to verify, and screenshots or demos as required). **Insufficient PR descriptions are grounds for rejection**, even if the code is present in the branch.

Each homework submission **MUST** also include in the repository:

### 📝 Required Documentation

| Item | Description |
|------|-------------|
| `README.md` | Clear explanation of your solution, approach, and AI tools used and *author* |
| `HOWTORUN.md` | Step-by-step guide to run your application |

### 📸 Screenshots *(Highly Expected)*

Include screenshots demonstrating:
- 🤖 AI tool interactions (prompts and responses)
- ✅ Your application running successfully
- 🧪 Test results (if applicable)
- 💡 Any interesting AI suggestions or corrections

> 📁 Place screenshots in the `docs/screenshots/` folder within each homework directory.

> 🎬 Provide runnable demo scripts where applicable.

---

## ▶️ How to Run Applications

Each homework folder should contain clear instructions. 

🔐 Environment Setup should be detailed to run the application.

Add 🧪 Testing guide.

---

## 📊 Grading Criteria

Your submissions will be evaluated on:

| Criteria | Weight | Description |
|----------|--------|-------------|
| ⚙️ **Functionality** | 30% | Does the code work as specified? |
| 📝 **AI Usage Documentation** | 25% | Clear documentation of how AI tools were used |
| 💻 **Code Quality** | 20% | Clean, readable, well-structured code |
| 📚 **Documentation** | 15% | README, comments, and explanations |
| 🎬 **Demo & Screenshots** | 10% | Visual evidence of working solution and AI interaction |

---

**🆘 Getting Help**

Contact the 📚instructor.

Collaborate with 👥 classmates (but submit individual work).

---


<div align="center">

### 🌟 Good luck with your assignments!

</div>
=======
Original course instructions: [COURSE.md](COURSE.md)
>>>>>>> Stashed changes
