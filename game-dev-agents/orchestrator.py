"""
Multi-Agent Game Development Orchestrator
==========================================
Coordinates Prometheus (Architect), Atlas (Backend), Iris (Frontend),
Hermes (NPC Logic), and Hephaestus (DevOps) to collaboratively build
a complete multiplayer game from a single high-level description.

Usage:
    python orchestrator.py
    python orchestrator.py --game "2D top-down shooter with 4 players"
    python orchestrator.py --agents architect,backend   # run specific agents
    python orchestrator.py --resume                     # re-run without re-architecting
"""

import os
import sys
import argparse
import time
import json
from config import OUTPUT_DIR, ANTHROPIC_API_KEY


def check_api_key():
    if not ANTHROPIC_API_KEY:
        print("ERROR: ANTHROPIC_API_KEY is not set.")
        print("  Set it in your environment or create a .env file:")
        print("  ANTHROPIC_API_KEY=sk-ant-...")
        sys.exit(1)


def ensure_output_dir():
    os.makedirs(OUTPUT_DIR, exist_ok=True)


def run_architect(game_description: str) -> str:
    from agents.architect import ArchitectAgent
    agent = ArchitectAgent()
    task = f"""
Design a complete multiplayer game based on this description:

"{game_description}"

Your deliverables:
1. Call save_game_spec with a full JSON game specification including:
   - name, genre, max_players, game_mechanics (list)
   - backend_tech: {{"websocket": true, "supabase_auth": true, "dynamodb_tables": [...], "lambda_functions": [...]}}
   - frontend_tech: {{"engine": "Three.js", "ui_framework": "Vanilla JS"}}
   - npc_types: [{{"name": "...", "states": [...], "has_dialogue": bool}}]
   - api_endpoints: [{{"method": "WS|GET|POST", "path": "...", "description": "..."}}]
   - websocket_messages: [{{"type": "...", "direction": "C2S|S2C|both", "payload_fields": [...]}}]
   - aws_services: ["DynamoDB", "Lambda", "API Gateway", ...]
   - dynamodb_tables: [{{"name": "...", "pk": "...", "sk": "...", "description": "..."}}]

2. Call append_to_design_doc to write a comprehensive game-design.md with sections:
   - Game Overview, Core Mechanics, Technical Architecture, Data Flow, Security Model

Make the design specific and implementable — not generic. Include exact table names,
Lambda function names, WebSocket message type byte values, and field names.
"""
    return agent.run(task)


def run_backend(game_description: str) -> str:
    from agents.backend import BackendAgent
    agent = BackendAgent()
    task = f"""
Generate the complete backend codebase for the multiplayer game.

Game: "{game_description}"

Steps:
1. Call load_game_spec to read the Architect's specification.
2. Call read_design_doc to understand the full design.
3. Generate and write these files using write_file:
   - backend/server.py         : Main WebSocket server using asyncio + websockets
   - backend/auth.py           : Supabase JWT verification middleware
   - backend/game_state.py     : Server-authoritative game state manager
   - backend/dynamodb.py       : DynamoDB client (leaderboard, inventory, sessions)
   - backend/lambda_handler.py : AWS Lambda entry points for API Gateway WS routes
   - backend/game_tick.py      : Game tick Lambda (runs every 100ms via EventBridge)
   - backend/requirements.txt  : All Python dependencies with pinned versions
   - backend/README.md         : Setup instructions and environment variables

4. Use generate_websocket_message_schema to create the message protocol, then write it
   as backend/message_types.py.

5. Include inline comments explaining:
   - Why each DynamoDB operation uses conditional expressions (race condition prevention)
   - How bit-packing reduces WebSocket bandwidth
   - How Lambda cold starts are mitigated
   - How the server validates player actions (anti-cheat)
"""
    return agent.run(task)


def run_frontend(game_description: str) -> str:
    from agents.frontend import FrontendAgent
    agent = FrontendAgent()
    task = f"""
Generate the complete frontend codebase for the multiplayer game.

Game: "{game_description}"

Steps:
1. Call load_game_spec to read the Architect's specification.
2. Call read_design_doc to understand the API contracts.
3. Generate and write these files using write_file:
   - frontend/index.html       : Complete HTML shell with Three.js CDN, CSS, and script imports
   - frontend/game.js          : Main game loop, Three.js scene, entity management
   - frontend/network.js       : WebSocket client with reconnect and message handling
   - frontend/auth.js          : Supabase sign-in/sign-up UI and JWT management
   - frontend/hud.js           : HUD overlay (health, score, leaderboard, chat)
   - frontend/input.js         : Keyboard/mouse/touch input handler
   - frontend/entities.js      : Player, NPC, and projectile entity classes

4. Use generate_threejs_entity to create entity classes, then incorporate them.

5. The game.js must implement:
   - Client-side prediction (apply input immediately, reconcile with server)
   - Position interpolation using lerp for remote players
   - Entity pool (reuse meshes instead of creating/destroying)
   - requestAnimationFrame game loop with fixed physics step

Include helpful comments on each Three.js optimization technique used.
"""
    return agent.run(task)


def run_npc(game_description: str) -> str:
    from agents.npc_agent import NPCAgent
    agent = NPCAgent()
    task = f"""
Generate the complete NPC AI system for the multiplayer game.

Game: "{game_description}"

Steps:
1. Call load_game_spec to read the Architect's NPC specifications.
2. Call generate_fsm for each NPC type defined in the spec.
3. Call generate_astar_pathfinder for the pathfinding system.
4. Write these files using write_file:
   - npc/npc_controller.py     : Main NPC update loop called by game tick Lambda
   - npc/fsm.py                : Finite state machine base classes
   - npc/pathfinder.py         : A* pathfinding (use generated code)
   - npc/sensing.py            : Line-of-sight, hearing range, threat scoring
   - npc/group_behavior.py     : Flanking, covering fire, retreat logic
   - npc/dialogue_agent.py     : Claude API integration for dynamic NPC speech
   - npc/spawn_manager.py      : NPC spawn zones, difficulty scaling, loot tables
   - npc/README.md             : How to add new NPC types

5. In npc/dialogue_agent.py, implement:
   - async get_npc_dialogue(npc_id, player_id, trigger) using claude-haiku-4-5 (fast+cheap)
   - Per-NPC conversation history stored in DynamoDB (last 10 turns)
   - Rate limiting: max 5 dialogue requests per player per minute
   - Fallback responses if API is unavailable
   - The NPC's personality defined by the game spec in its system prompt
"""
    return agent.run(task)


def run_devops(game_description: str) -> str:
    from agents.devops import DevOpsAgent
    agent = DevOpsAgent()
    task = f"""
Generate the complete DevOps infrastructure for the multiplayer game.

Game: "{game_description}"

Steps:
1. Call load_game_spec to read all AWS service requirements.
2. Use generate_dynamodb_table for each table in the spec.
3. Write these files using write_file:
   - deploy/template.yaml          : AWS SAM template (all Lambda, DynamoDB, API GW resources)
   - deploy/deploy.sh              : Deployment script with sam build + sam deploy
   - deploy/rollback.sh            : Rollback to previous Lambda version
   - docker/docker-compose.yml     : Local dev environment (DynamoDB-local, backend, nginx)
   - docker/Dockerfile.backend     : Python backend container
   - docker/nginx.conf             : Reverse proxy config for WS and static files
   - .github/workflows/deploy.yml  : CI/CD pipeline (test → staging → prod)
   - deploy/monitoring.py          : CloudWatch alarms + X-Ray tracing setup
   - deploy/seed_data.py           : Seed DynamoDB with test data for local dev
   - deploy/README.md              : Step-by-step deployment guide

4. Include in template.yaml:
   - All Lambda functions from the spec with proper IAM roles
   - API Gateway WebSocket API with connection management
   - All DynamoDB tables with GSIs, encryption, PITR enabled
   - CloudWatch Log Groups with 30-day retention
   - Cost estimation comments (approximate monthly cost at 100 concurrent players)

5. Make deploy.sh idempotent — safe to run multiple times.
"""
    return agent.run(task)


# ------------------------------------------------------------------
# Orchestrator pipeline
# ------------------------------------------------------------------

PIPELINE = [
    ("architect",  "Designing game architecture...",        run_architect),
    ("backend",    "Building backend server code...",       run_backend),
    ("frontend",   "Building frontend client code...",      run_frontend),
    ("npc",        "Building NPC AI systems...",            run_npc),
    ("devops",     "Generating DevOps infrastructure...",   run_devops),
]


def run_pipeline(game_description: str, selected_agents: list[str], resume: bool):
    print("\n" + "="*70)
    print("  MULTIPLAYER GAME DEV — MULTI-AGENT SYSTEM")
    print("  Powered by Claude Opus 4.6 with Adaptive Thinking")
    print("="*70)
    print(f"\n  Game: {game_description}")
    print(f"  Output: {OUTPUT_DIR}\n")

    results = {}
    total_start = time.time()

    for agent_key, status_msg, runner in PIPELINE:
        if selected_agents and agent_key not in selected_agents:
            print(f"  [SKIP] {agent_key}")
            continue

        if resume and agent_key == "architect":
            # In resume mode, check if spec already exists
            spec_path = os.path.join(OUTPUT_DIR, "game-spec.json")
            if os.path.exists(spec_path):
                print(f"  [SKIP] architect (resuming — spec exists)")
                continue

        print(f"\n  [{agent_key.upper()}] {status_msg}")
        t0 = time.time()
        try:
            result = runner(game_description)
            elapsed = time.time() - t0
            results[agent_key] = {"status": "ok", "elapsed": round(elapsed, 1)}
            print(f"  [{agent_key.upper()}] Complete ({elapsed:.1f}s)")
        except Exception as e:
            elapsed = time.time() - t0
            results[agent_key] = {"status": "error", "error": str(e), "elapsed": round(elapsed, 1)}
            print(f"  [{agent_key.upper()}] ERROR: {e}")

    # Summary
    total = time.time() - total_start
    print("\n" + "="*70)
    print("  BUILD SUMMARY")
    print("="*70)
    for key, r in results.items():
        icon = "✓" if r["status"] == "ok" else "✗"
        print(f"  {icon} {key:15} {r['elapsed']:5.1f}s  {r.get('error', '')}")
    print(f"\n  Total time: {total:.1f}s")

    # List generated files
    from tools.file_tools import list_files
    file_list = list_files()
    if file_list.get("files"):
        print(f"\n  Generated {len(file_list['files'])} files in: {OUTPUT_DIR}/")
        for f in sorted(file_list["files"])[:30]:
            print(f"    {f}")
        if len(file_list["files"]) > 30:
            print(f"    ... and {len(file_list['files']) - 30} more")

    print("\n  Done! Open the generated_game/ directory to explore your game.\n")
    return results


def main():
    parser = argparse.ArgumentParser(
        description="Multi-Agent Multiplayer Game Developer"
    )
    parser.add_argument(
        "--game",
        type=str,
        default=(
            "A 3D multiplayer dungeon crawler for up to 6 players. Players choose "
            "from Warrior, Mage, or Rogue classes. The dungeon has procedurally placed "
            "rooms with goblin and skeleton NPCs. Players collect loot, level up, and "
            "compete on a leaderboard by floors cleared. Chat is available in-game."
        ),
        help="Natural language description of the game to build"
    )
    parser.add_argument(
        "--agents",
        type=str,
        default="",
        help="Comma-separated list of agents to run (architect,backend,frontend,npc,devops). Default: all"
    )
    parser.add_argument(
        "--resume",
        action="store_true",
        help="Resume a previous build (skip architect if spec exists)"
    )

    args = parser.parse_args()
    selected = [a.strip() for a in args.agents.split(",") if a.strip()] if args.agents else []

    check_api_key()
    ensure_output_dir()
    run_pipeline(args.game, selected, args.resume)


if __name__ == "__main__":
    main()
