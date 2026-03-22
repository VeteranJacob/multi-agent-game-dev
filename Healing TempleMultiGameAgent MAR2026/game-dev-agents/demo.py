"""
Quick demo: run only the Architect agent to generate the game spec,
then show a preview of what all agents would produce.
Run this first to verify your API key works before the full pipeline.

    python demo.py
"""

import os
import sys
import json

# Make sure we can import from the project root
sys.path.insert(0, os.path.dirname(__file__))

from config import ANTHROPIC_API_KEY, OUTPUT_DIR


def main():
    if not ANTHROPIC_API_KEY:
        print("ERROR: Set ANTHROPIC_API_KEY in your environment or .env file.")
        sys.exit(1)

    os.makedirs(OUTPUT_DIR, exist_ok=True)

    game = (
        "A real-time multiplayer top-down space shooter for up to 8 players. "
        "Players pilot spaceships and fight over asteroid mining rights. "
        "NPCs include pirate drones and a boss alien ship. "
        "Leaderboard tracks total ore mined and kills."
    )

    print("\n" + "="*60)
    print("  DEMO: Architecture Agent Only")
    print("="*60)
    print(f"\n  Game: {game}\n")

    from agents.architect import ArchitectAgent
    agent = ArchitectAgent()
    task = f"""
Design a complete multiplayer game based on this description:

"{game}"

Your deliverables:
1. Call save_game_spec with a full JSON game specification.
2. Call append_to_design_doc with a "Game Overview" section.
3. Call append_to_design_doc with a "Technical Architecture" section covering:
   - WebSocket message types with byte values
   - DynamoDB table schemas
   - Lambda function list

Be specific and detailed — this spec drives all other agents.
"""

    result = agent.run(task, verbose=True)

    # Show the spec that was generated
    spec_path = os.path.join(OUTPUT_DIR, "game-spec.json")
    if os.path.exists(spec_path):
        print("\n" + "="*60)
        print("  Generated game-spec.json:")
        print("="*60)
        with open(spec_path) as f:
            spec = json.load(f)
        print(json.dumps(spec, indent=2)[:2000])
        if len(json.dumps(spec)) > 2000:
            print("  ... (truncated, see game-spec.json for full spec)")

    print("\n  Demo complete!")
    print(f"  Files written to: {OUTPUT_DIR}")
    print("\n  To run the full pipeline:")
    print('    python orchestrator.py --game "your game description"')
    print("\n  To run specific agents only:")
    print('    python orchestrator.py --agents backend,frontend')


if __name__ == "__main__":
    main()
