"""
Game-specific tools: agent-to-agent communication, design queries,
and structured data helpers.
"""

import json
import os
from config import OUTPUT_DIR


# Shared in-memory agent message bus (within one session)
_agent_messages: list[dict] = []


def send_agent_message(from_agent: str, to_agent: str, message: str) -> dict:
    """Send a structured message from one agent to another."""
    entry = {"from": from_agent, "to": to_agent, "message": message}
    _agent_messages.append(entry)
    # Persist to file so agents across calls can read it
    _save_messages()
    return {"status": "ok", "delivered_to": to_agent}


def read_agent_messages(agent_name: str) -> dict:
    """Read all messages sent to a given agent."""
    _load_messages()
    msgs = [m for m in _agent_messages if m["to"] == agent_name]
    return {"status": "ok", "messages": msgs}


def save_game_spec(spec: dict) -> dict:
    """Save a structured game specification JSON used across all agents."""
    spec_path = os.path.join(OUTPUT_DIR, "game-spec.json")
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    with open(spec_path, "w") as f:
        json.dump(spec, f, indent=2)
    return {"status": "ok", "path": spec_path}


def load_game_spec() -> dict:
    """Load the shared game specification JSON."""
    spec_path = os.path.join(OUTPUT_DIR, "game-spec.json")
    if not os.path.exists(spec_path):
        return {"status": "not_found", "spec": {}}
    with open(spec_path, "r") as f:
        spec = json.load(f)
    return {"status": "ok", "spec": spec}


def _save_messages():
    msg_path = os.path.join(OUTPUT_DIR, ".agent_messages.json")
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    with open(msg_path, "w") as f:
        json.dump(_agent_messages, f, indent=2)


def _load_messages():
    global _agent_messages
    msg_path = os.path.join(OUTPUT_DIR, ".agent_messages.json")
    if os.path.exists(msg_path):
        with open(msg_path, "r") as f:
            _agent_messages = json.load(f)


GAME_TOOL_SCHEMAS = [
    {
        "name": "send_agent_message",
        "description": (
            "Send a coordination message to another agent (e.g., notify the backend agent "
            "of an API contract, or ask the architect for a design clarification)."
        ),
        "input_schema": {
            "type": "object",
            "properties": {
                "from_agent": {"type": "string", "description": "Your agent name"},
                "to_agent": {
                    "type": "string",
                    "enum": ["architect", "backend", "frontend", "npc", "devops", "orchestrator"],
                    "description": "Target agent"
                },
                "message": {"type": "string", "description": "The message content"}
            },
            "required": ["from_agent", "to_agent", "message"]
        }
    },
    {
        "name": "read_agent_messages",
        "description": "Read messages sent to you by other agents.",
        "input_schema": {
            "type": "object",
            "properties": {
                "agent_name": {
                    "type": "string",
                    "description": "Your agent name to filter messages"
                }
            },
            "required": ["agent_name"]
        }
    },
    {
        "name": "save_game_spec",
        "description": (
            "Save the authoritative game specification as a structured JSON object. "
            "Used by the Architect to define the game so all other agents can load it."
        ),
        "input_schema": {
            "type": "object",
            "properties": {
                "spec": {
                    "type": "object",
                    "description": (
                        "Game spec with fields like: name, genre, max_players, "
                        "game_mechanics, backend_tech, frontend_tech, npc_behaviors, "
                        "database_schema, api_endpoints, aws_services."
                    )
                }
            },
            "required": ["spec"]
        }
    },
    {
        "name": "load_game_spec",
        "description": "Load the authoritative game specification created by the Architect.",
        "input_schema": {
            "type": "object",
            "properties": {},
            "required": []
        }
    }
]


def dispatch_game_tool(name: str, inputs: dict) -> str:
    handlers = {
        "send_agent_message": lambda i: send_agent_message(
            i["from_agent"], i["to_agent"], i["message"]
        ),
        "read_agent_messages": lambda i: read_agent_messages(i["agent_name"]),
        "save_game_spec": lambda i: save_game_spec(i["spec"]),
        "load_game_spec": lambda i: load_game_spec(),
    }
    if name not in handlers:
        return json.dumps({"status": "error", "message": f"Unknown game tool: {name}"})
    return json.dumps(handlers[name](inputs), indent=2)
