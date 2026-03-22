"""
Hermes — The NPC Logic Agent
Generates AI-driven NPC behavior: state machines, pathfinding,
decision trees, and dynamic narrative responses via Claude API integration.
"""

import json
from agents.base_agent import BaseAgent

SYSTEM_PROMPT = """
You are Hermes, the NPC Logic Agent for a multiplayer game dev team.

Your responsibilities:
1. Design NPC behavior systems:
   - Finite state machines (FSM): Idle → Patrol → Chase → Attack → Flee states.
   - Hierarchical Task Network (HTN) planning for complex NPC goals.
   - Behavior trees as an alternative for boss enemies.

2. Generate Python NPC AI code (runs server-side in Lambda or game server):
   - NPCController class with update() method called each game tick.
   - Pathfinding using A* on a grid or navigation mesh.
   - Sensing: line-of-sight checks, hearing radius, threat assessment.
   - Group behaviors: flanking, covering, retreating under fire.

3. Generate Claude API integration for dynamic NPC dialogue (npc/dialogue_agent.py):
   - Use the Anthropic Python SDK to generate context-aware NPC speech.
   - Cache dialogue responses in DynamoDB to reduce API calls.
   - Maintain per-NPC conversation history for continuity.
   - Rate-limit per-player NPC interactions (max 5 per minute).

4. Generate NPC spawn and lifecycle management:
   - Spawn zones with difficulty scaling based on player count.
   - Respawn timers with exponential backoff.
   - Loot table generation on death.

Always load_game_spec first to understand the game design.
Write NPC files into the npc/ subdirectory.
Use Python 3.12 with type hints throughout.
Comment every decision-making step for future maintainability.
"""

NPC_EXTRA_TOOLS = [
    {
        "name": "generate_fsm",
        "description": "Generate a Python finite state machine class for an NPC type.",
        "input_schema": {
            "type": "object",
            "properties": {
                "npc_type": {"type": "string", "description": "NPC type name, e.g. 'Goblin', 'Boss', 'Merchant'"},
                "states": {
                    "type": "array",
                    "items": {"type": "string"},
                    "description": "List of state names, e.g. ['IDLE', 'PATROL', 'CHASE', 'ATTACK', 'FLEE']"
                },
                "has_dialogue": {"type": "boolean", "description": "Whether this NPC can speak via Claude API"}
            },
            "required": ["npc_type", "states"]
        }
    },
    {
        "name": "generate_astar_pathfinder",
        "description": "Generate a Python A* pathfinding implementation for NPC navigation.",
        "input_schema": {
            "type": "object",
            "properties": {
                "grid_based": {
                    "type": "boolean",
                    "description": "True for grid, False for waypoint graph"
                },
                "allow_diagonal": {
                    "type": "boolean",
                    "description": "Whether diagonal movement is allowed (grid only)"
                }
            },
            "required": ["grid_based"]
        }
    }
]


class NPCAgent(BaseAgent):
    name = "Hermes (NPC Logic Agent)"
    agent_key = "npc"
    system_prompt = SYSTEM_PROMPT
    extra_tools = NPC_EXTRA_TOOLS

    def _dispatch_extra_tool(self, name: str, inputs: dict) -> str:
        if name == "generate_fsm":
            return self._gen_fsm(inputs)
        if name == "generate_astar_pathfinder":
            return self._gen_astar(inputs)
        return super()._dispatch_extra_tool(name, inputs)

    def _gen_fsm(self, inputs: dict) -> str:
        npc = inputs["npc_type"]
        states = inputs.get("states", ["IDLE", "PATROL", "CHASE", "ATTACK"])
        has_dialogue = inputs.get("has_dialogue", False)

        enum_lines = "\n".join(f"    {s} = '{s}'" for s in states)
        transitions = "\n".join(
            f"        if self.state == State.{s}:\n"
            f"            self._handle_{s.lower()}(context)"
            for s in states
        )
        handlers = "\n\n".join(
            f"    def _handle_{s.lower()}(self, context: dict):\n"
            f"        # TODO: implement {s} behavior\n"
            f"        pass"
            for s in states
        )
        dialogue_import = "from npc.dialogue_agent import get_npc_dialogue\n" if has_dialogue else ""
        dialogue_method = (
            "\n\n    async def speak(self, player_id: str, trigger: str) -> str:\n"
            "        return await get_npc_dialogue(self.npc_id, player_id, trigger)\n"
            if has_dialogue else ""
        )

        code = f"""{dialogue_import}from enum import Enum
from dataclasses import dataclass, field
from typing import Optional
import math


class State(Enum):
{enum_lines}


@dataclass
class {npc}NPC:
    npc_id: str
    position: list[float] = field(default_factory=lambda: [0.0, 0.0, 0.0])
    health: float = 100.0
    state: State = State.{states[0]}
    target_player_id: Optional[str] = None
    patrol_waypoints: list[list[float]] = field(default_factory=list)
    _waypoint_index: int = 0

    def update(self, context: dict, delta: float) -> None:
        \"\"\"Called every game tick. context contains nearby players, obstacles, etc.\"\"\"
{transitions}

{handlers}{dialogue_method}
    def take_damage(self, amount: float) -> None:
        self.health = max(0.0, self.health - amount)
        if self.health <= 0:
            self.state = State.{states[-1] if len(states) > 1 else states[0]}

    def distance_to(self, pos: list[float]) -> float:
        return math.sqrt(sum((a - b) ** 2 for a, b in zip(self.position, pos)))
"""
        return json.dumps({"status": "ok", "fsm_code": code})

    def _gen_astar(self, inputs: dict) -> str:
        grid_based = inputs.get("grid_based", True)
        diagonal = inputs.get("allow_diagonal", True)

        if grid_based:
            diag_neighbors = """
        if allow_diagonal:
            for dx in [-1, 0, 1]:
                for dy in [-1, 0, 1]:
                    if dx == 0 and dy == 0:
                        continue
                    nx, ny = x + dx, y + dy
                    cost = 1.414 if dx != 0 and dy != 0 else 1.0
                    yield (nx, ny), cost""" if diagonal else """
        for dx, dy in [(-1,0),(1,0),(0,-1),(0,1)]:
            yield (x + dx, y + dy), 1.0"""

            code = f"""import heapq
from typing import Optional


def astar(grid: list[list[int]], start: tuple[int,int], goal: tuple[int,int],
          allow_diagonal: bool = {str(diagonal)}) -> Optional[list[tuple[int,int]]]:
    \"\"\"A* pathfinding on a 2D grid. 0=walkable, 1=obstacle.\"\"\"
    rows, cols = len(grid), len(grid[0])

    def heuristic(a, b):
        return ((a[0]-b[0])**2 + (a[1]-b[1])**2) ** 0.5

    def neighbors(pos):
        x, y = pos{diag_neighbors}
        if 0 <= nx < rows and 0 <= ny < cols and grid[nx][ny] == 0:
            yield (nx, ny), cost

    open_set = [(0, start)]
    came_from = {{}}
    g_score = {{start: 0.0}}

    while open_set:
        _, current = heapq.heappop(open_set)
        if current == goal:
            path = []
            while current in came_from:
                path.append(current)
                current = came_from[current]
            path.append(start)
            return path[::-1]
        for neighbor, cost in neighbors(current):
            tentative_g = g_score[current] + cost
            if tentative_g < g_score.get(neighbor, float('inf')):
                came_from[neighbor] = current
                g_score[neighbor] = tentative_g
                f = tentative_g + heuristic(neighbor, goal)
                heapq.heappush(open_set, (f, neighbor))
    return None  # No path found
"""
        else:
            code = """import heapq
from typing import Optional


class WaypointGraph:
    def __init__(self):
        self.edges: dict[str, list[tuple[str, float]]] = {}

    def add_edge(self, a: str, b: str, cost: float, bidirectional: bool = True):
        self.edges.setdefault(a, []).append((b, cost))
        if bidirectional:
            self.edges.setdefault(b, []).append((a, cost))

    def astar(self, start: str, goal: str, heuristic=None) -> Optional[list[str]]:
        h = heuristic or (lambda a, b: 0)
        open_set = [(0, start)]
        came_from = {}
        g_score = {start: 0.0}
        while open_set:
            _, current = heapq.heappop(open_set)
            if current == goal:
                path = []
                while current in came_from:
                    path.append(current)
                    current = came_from[current]
                return [start] + path[::-1]
            for neighbor, cost in self.edges.get(current, []):
                tg = g_score[current] + cost
                if tg < g_score.get(neighbor, float('inf')):
                    came_from[neighbor] = current
                    g_score[neighbor] = tg
                    heapq.heappush(open_set, (tg + h(neighbor, goal), neighbor))
        return None
"""
        return json.dumps({"status": "ok", "pathfinder_code": code})
