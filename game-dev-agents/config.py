"""
Configuration for the Multiplayer Game Dev Multi-Agent System.
Set ANTHROPIC_API_KEY in your environment or a .env file.
"""

import os
from dotenv import load_dotenv

load_dotenv()

ANTHROPIC_API_KEY = os.getenv("ANTHROPIC_API_KEY", "")
MODEL = "claude-opus-4-6"
MAX_TOKENS = 16000

# Output directory where agents write generated game files
OUTPUT_DIR = os.path.join(os.path.dirname(__file__), "generated_game")

# Agent names
AGENTS = {
    "architect":  "Prometheus (Architect)",
    "backend":    "Atlas (Backend Specialist)",
    "frontend":   "Iris (Frontend Specialist)",
    "npc":        "Hermes (NPC Logic Agent)",
    "devops":     "Hephaestus (DevOps Agent)",
}
