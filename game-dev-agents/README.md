# Multiplayer Game Dev — Multi-Agent System

A team of AI agents built on **Claude Opus 4.6** that collaboratively design and
generate a complete multiplayer game codebase from a single description.

## The Agent Team

| Agent | Codename | Responsibility |
|-------|----------|----------------|
| Architect | **Prometheus** | Game design, data models, API contracts, tech choices |
| Backend Specialist | **Atlas** | WebSocket server, Supabase auth, DynamoDB, Lambda |
| Frontend Specialist | **Iris** | Three.js 3D scene, WebSocket client, HUD, auth UI |
| NPC Logic Agent | **Hermes** | AI state machines, A* pathfinding, Claude-powered dialogue |
| DevOps Agent | **Hephaestus** | SAM templates, Docker, CI/CD, CloudWatch monitoring |

Each agent uses an **autonomous tool-use loop** — it reads the shared design spec,
calls code-generation tools, writes files, and coordinates with other agents.

## Architecture

```
orchestrator.py
    │
    ├── Prometheus (Architect)
    │       └── saves game-spec.json + game-design.md
    │
    ├── Atlas (Backend)         reads spec → generates backend/
    │       ├── server.py       WebSocket server (asyncio + websockets)
    │       ├── auth.py         Supabase JWT middleware
    │       ├── dynamodb.py     DynamoDB: leaderboard, inventory, sessions
    │       ├── lambda_handler.py  API GW WebSocket Lambda handlers
    │       └── game_tick.py    Periodic game state Lambda
    │
    ├── Iris (Frontend)         reads spec → generates frontend/
    │       ├── index.html      Complete game shell
    │       ├── game.js         Three.js main loop + client prediction
    │       ├── network.js      WebSocket client with reconnect
    │       ├── auth.js         Supabase sign-in/sign-up
    │       └── hud.js          Health, score, leaderboard, chat
    │
    ├── Hermes (NPC)            reads spec → generates npc/
    │       ├── npc_controller.py  FSM update loop
    │       ├── pathfinder.py      A* on grid or waypoint graph
    │       ├── dialogue_agent.py  Claude API NPC speech (claude-haiku-4-5)
    │       └── spawn_manager.py   Difficulty scaling + loot tables
    │
    └── Hephaestus (DevOps)    reads spec → generates deploy/
            ├── template.yaml   AWS SAM (Lambda, DynamoDB, API Gateway)
            ├── docker-compose.yml  Local dev environment
            └── deploy.yml      GitHub Actions CI/CD
```

## Setup

```bash
# 1. Install dependencies
pip install -r requirements.txt

# 2. Set your Anthropic API key
export ANTHROPIC_API_KEY="sk-ant-..."
# or create a .env file:
echo "ANTHROPIC_API_KEY=sk-ant-..." > .env

# 3. Quick demo (Architect only, ~2 min)
python demo.py

# 4. Full pipeline (all 5 agents, ~10-20 min)
python orchestrator.py --game "your game description"

# 5. Run specific agents
python orchestrator.py --agents backend,frontend

# 6. Resume a previous build (skip re-architecting)
python orchestrator.py --resume
```

## Example Games

```bash
# Dungeon crawler (default)
python orchestrator.py

# Space shooter
python orchestrator.py --game "Multiplayer top-down space shooter for 8 players.
  Spaceships battle over asteroid mining zones. Boss alien NPC."

# Battle royale
python orchestrator.py --game "100-player battle royale. Shrinking safe zone,
  loot crates, vehicles. NPC bandits in abandoned areas."

# MMORPG starter
python orchestrator.py --game "Fantasy MMORPG for 50 players. Three classes:
  Warrior, Mage, Rogue. Dungeon raids, crafting, guild system."
```

## Technology Stack Generated

| Layer | Technology |
|-------|-----------|
| Real-time networking | WebSockets (asyncio + websockets) |
| Auth | Supabase (email/JWT) |
| Player data & leaderboards | AWS DynamoDB |
| Serverless game logic | AWS Lambda + API Gateway |
| Auto-scaling | DynamoDB on-demand + Lambda concurrency |
| 3D rendering | Three.js r158+ |
| NPC dialogue | Claude API (claude-haiku-4-5, fast & cheap) |
| Local dev | Docker Compose + DynamoDB-local |
| CI/CD | GitHub Actions + AWS SAM |
| Monitoring | CloudWatch + X-Ray |

## Agent Communication

Agents coordinate via:
- **game-spec.json** — authoritative spec saved by Architect, read by all others
- **game-design.md** — shared design document with sections appended by each agent
- **agent_messages** — in-session message bus for cross-agent notifications

## Cost Estimate

Each full pipeline run (5 agents) uses approximately:
- ~150,000 input tokens + ~200,000 output tokens
- At Claude Opus 4.6 pricing ($5/$25 per 1M): roughly **$5–7 per full run**
- Use `--agents architect` first to validate the design cheaply (~$0.50)

## Output Structure

```
generated_game/
├── game-spec.json          # Structured game specification
├── game-design.md          # Human-readable design document
├── backend/
│   ├── server.py
│   ├── auth.py
│   ├── dynamodb.py
│   ├── lambda_handler.py
│   ├── message_types.py
│   └── requirements.txt
├── frontend/
│   ├── index.html
│   ├── game.js
│   ├── network.js
│   ├── auth.js
│   └── hud.js
├── npc/
│   ├── npc_controller.py
│   ├── pathfinder.py
│   ├── dialogue_agent.py
│   └── spawn_manager.py
└── deploy/
    ├── template.yaml
    ├── docker-compose.yml
    └── .github/workflows/deploy.yml
```
