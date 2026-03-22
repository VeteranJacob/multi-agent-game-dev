"""
Iris — The Frontend Specialist Agent
Builds the client-side game: Three.js 3D scene, WebSocket client,
Supabase auth UI, and HUD rendering.
"""

import json
from agents.base_agent import BaseAgent

SYSTEM_PROMPT = """
You are Iris, the Frontend Specialist Agent for a multiplayer game dev team.

Your responsibilities:
1. Generate a Three.js game client (ES6 modules, index.html + game.js):
   - 3D scene setup: renderer, camera, lighting, shadow maps.
   - Player entity rendering: load GLTF models or use procedural geometry.
   - Real-time position interpolation (lerp) for smooth remote-player movement.
   - Input handling: keyboard WASD + mouse look, mobile touch fallback.

2. Generate the WebSocket client (client/network.js):
   - Connect to the backend WebSocket server with auto-reconnect (exponential backoff).
   - Send player movement/action messages in the same bit-packed format as the backend.
   - Receive and apply server-authoritative state updates.
   - Handle latency compensation with client-side prediction.

3. Generate the Supabase auth UI (client/auth.js):
   - Email/password sign-up and sign-in.
   - JWT token storage and refresh.
   - Pass JWT to WebSocket server on connect for authentication.

4. Generate the HUD (client/hud.js):
   - Health bar, score display, mini-map, player count.
   - Leaderboard panel that polls the REST API.
   - Chat input that sends via WebSocket.

Always load_game_spec first to understand the game design.
Write all files into the frontend/ subdirectory.
Use vanilla JS (no frameworks) with Three.js r158+ CDN imports.
Include a complete index.html that ties everything together.
Add CSS for a dark-themed game UI.
"""

FRONTEND_EXTRA_TOOLS = [
    {
        "name": "generate_threejs_entity",
        "description": "Generate a Three.js entity class (player, NPC, projectile, etc.) as a JS class string.",
        "input_schema": {
            "type": "object",
            "properties": {
                "entity_name": {"type": "string", "description": "Class name, e.g. Player, NPC, Bullet"},
                "geometry": {
                    "type": "string",
                    "enum": ["BoxGeometry", "SphereGeometry", "CylinderGeometry", "GLTF"],
                    "description": "Three.js geometry type to use"
                },
                "color": {"type": "string", "description": "Hex color string, e.g. #4488ff"},
                "has_physics": {"type": "boolean", "description": "Include basic velocity/gravity physics"}
            },
            "required": ["entity_name", "geometry"]
        }
    }
]


class FrontendAgent(BaseAgent):
    name = "Iris (Frontend Specialist)"
    agent_key = "frontend"
    system_prompt = SYSTEM_PROMPT
    extra_tools = FRONTEND_EXTRA_TOOLS

    def _dispatch_extra_tool(self, name: str, inputs: dict) -> str:
        if name == "generate_threejs_entity":
            return self._gen_entity(inputs)
        return super()._dispatch_extra_tool(name, inputs)

    def _gen_entity(self, inputs: dict) -> str:
        ename = inputs["entity_name"]
        geo = inputs.get("geometry", "BoxGeometry")
        color = inputs.get("color", "#ffffff")
        physics = inputs.get("has_physics", False)

        geo_line = {
            "BoxGeometry": "new THREE.BoxGeometry(1, 1, 1)",
            "SphereGeometry": "new THREE.SphereGeometry(0.5, 16, 16)",
            "CylinderGeometry": "new THREE.CylinderGeometry(0.4, 0.4, 1.8, 16)",
            "GLTF": "null  // set via loadGLTF()",
        }.get(geo, "new THREE.BoxGeometry(1, 1, 1)")

        physics_code = ""
        if physics:
            physics_code = """
  update(delta) {
    this.velocity.y -= 9.8 * delta;  // gravity
    this.mesh.position.addScaledVector(this.velocity, delta);
    if (this.mesh.position.y <= 0) {
      this.mesh.position.y = 0;
      this.velocity.y = 0;
    }
  }"""

        code = f"""export class {ename} {{
  constructor(scene, id) {{
    this.id = id;
    this.velocity = new THREE.Vector3();
    const geometry = {geo_line};
    const material = new THREE.MeshStandardMaterial({{ color: {repr(color)} }});
    this.mesh = new THREE.Mesh(geometry, material);
    this.mesh.castShadow = true;
    this.mesh.receiveShadow = true;
    scene.add(this.mesh);
  }}

  setPosition(x, y, z) {{
    this.mesh.position.set(x, y, z);
  }}

  lerpTo(x, y, z, alpha = 0.1) {{
    this.mesh.position.lerp(new THREE.Vector3(x, y, z), alpha);
  }}

  dispose(scene) {{
    scene.remove(this.mesh);
    this.mesh.geometry.dispose();
    this.mesh.material.dispose();
  }}{physics_code}
}}
"""
        return json.dumps({"status": "ok", "entity_code": code})
