"""
Atlas — The Backend Specialist Agent
Generates all server-side code: WebSocket server, Supabase auth, DynamoDB handlers,
and AWS Lambda entry points.
"""

import json
from agents.base_agent import BaseAgent
from tools.file_tools import FILE_TOOL_SCHEMAS, dispatch_file_tool
from tools.game_tools import GAME_TOOL_SCHEMAS, dispatch_game_tool

SYSTEM_PROMPT = """
You are Atlas, the Backend Specialist Agent for a multiplayer game dev team.

Your responsibilities:
1. Generate a Python WebSocket server (using `websockets` library) that:
   - Handles player connections, disconnects, and real-time position/state sync.
   - Uses bit-packing for message types to minimize bandwidth (1-byte message type prefix).
   - Broadcasts state updates to all connected players efficiently.
   - Validates incoming messages to prevent cheating.

2. Generate Supabase integration code:
   - Player authentication (JWT verification middleware).
   - Row Level Security policy comments in SQL.
   - Player profile CRUD operations.

3. Generate DynamoDB handler code:
   - Leaderboard table with atomic score updates (conditional expressions).
   - Player inventory with optimistic locking.
   - Session/game-state table for server-side persistence.

4. Generate AWS Lambda entry points:
   - Game tick Lambda (periodic game state updates).
   - API Gateway WebSocket route handlers ($connect, $disconnect, $default).
   - Leaderboard query Lambda.

Always load_game_spec first to understand the game design.
Write all files into the backend/ subdirectory.
Use Python 3.12 syntax and include requirements.txt for the backend.
Add inline comments explaining networking and database design choices.
"""

# Extra backend-specific tools
BACKEND_EXTRA_TOOLS = [
    {
        "name": "generate_websocket_message_schema",
        "description": (
            "Generate a bit-packed WebSocket message schema definition "
            "as a Python dataclass or enum. Returns the schema code string."
        ),
        "input_schema": {
            "type": "object",
            "properties": {
                "message_types": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "List of message type names (e.g. MOVE, SHOOT, CHAT)"
                },
                "include_serialization": {
                    "type": "boolean",
                    "description": "Whether to include pack/unpack helper functions"
                }
            },
            "required": ["message_types"]
        }
    }
]


class BackendAgent(BaseAgent):
    name = "Atlas (Backend Specialist)"
    agent_key = "backend"
    system_prompt = SYSTEM_PROMPT
    extra_tools = BACKEND_EXTRA_TOOLS

    def _dispatch_extra_tool(self, name: str, inputs: dict) -> str:
        if name == "generate_websocket_message_schema":
            return self._gen_ws_schema(inputs)
        return super()._dispatch_extra_tool(name, inputs)

    def _gen_ws_schema(self, inputs: dict) -> str:
        types = inputs.get("message_types", [])
        include_ser = inputs.get("include_serialization", True)

        lines = [
            "import struct",
            "from enum import IntEnum",
            "",
            "class MsgType(IntEnum):",
        ]
        for i, t in enumerate(types):
            lines.append(f"    {t.upper()} = {i}")

        if include_ser:
            lines += [
                "",
                "def pack_message(msg_type: MsgType, payload: bytes) -> bytes:",
                "    # 1-byte type prefix + variable payload",
                "    return struct.pack('!B', int(msg_type)) + payload",
                "",
                "def unpack_message(data: bytes) -> tuple[MsgType, bytes]:",
                "    msg_type = MsgType(struct.unpack('!B', data[:1])[0])",
                "    return msg_type, data[1:]",
            ]

        code = "\n".join(lines)
        return json.dumps({"status": "ok", "schema_code": code})
