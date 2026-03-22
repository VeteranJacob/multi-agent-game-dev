"""
File system tools shared across all agents.
These tools let agents write, read, and list generated game files.
"""

import os
import json
from config import OUTPUT_DIR


def write_file(relative_path: str, content: str) -> dict:
    """Write a file to the generated_game output directory."""
    abs_path = os.path.join(OUTPUT_DIR, relative_path)
    os.makedirs(os.path.dirname(abs_path), exist_ok=True)
    with open(abs_path, "w", encoding="utf-8") as f:
        f.write(content)
    return {"status": "ok", "path": abs_path, "bytes_written": len(content)}


def read_file(relative_path: str) -> dict:
    """Read a file from the generated_game output directory."""
    abs_path = os.path.join(OUTPUT_DIR, relative_path)
    if not os.path.exists(abs_path):
        return {"status": "error", "message": f"File not found: {relative_path}"}
    with open(abs_path, "r", encoding="utf-8") as f:
        content = f.read()
    return {"status": "ok", "path": abs_path, "content": content}


def list_files(sub_dir: str = "") -> dict:
    """List all files under the generated_game directory (or a subdirectory)."""
    scan_dir = os.path.join(OUTPUT_DIR, sub_dir)
    if not os.path.exists(scan_dir):
        return {"status": "ok", "files": []}
    files = []
    for root, _, filenames in os.walk(scan_dir):
        for fname in filenames:
            full = os.path.join(root, fname)
            rel = os.path.relpath(full, OUTPUT_DIR)
            files.append(rel.replace("\\", "/"))
    return {"status": "ok", "files": files}


def append_to_design_doc(section: str, content: str) -> dict:
    """Append a section to the shared game-design.md document."""
    doc_path = os.path.join(OUTPUT_DIR, "game-design.md")
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    with open(doc_path, "a", encoding="utf-8") as f:
        f.write(f"\n## {section}\n\n{content}\n")
    return {"status": "ok", "section_added": section}


def read_design_doc() -> dict:
    """Read the shared game-design.md document."""
    return read_file("game-design.md")


# Tool schemas for Anthropic tool use API
FILE_TOOL_SCHEMAS = [
    {
        "name": "write_file",
        "description": (
            "Write a source code file into the generated game project. "
            "Use relative paths like 'backend/server.py' or 'frontend/game.js'."
        ),
        "input_schema": {
            "type": "object",
            "properties": {
                "relative_path": {
                    "type": "string",
                    "description": "Path relative to the game project root, e.g. 'backend/server.py'"
                },
                "content": {
                    "type": "string",
                    "description": "Full file content to write"
                }
            },
            "required": ["relative_path", "content"]
        }
    },
    {
        "name": "read_file",
        "description": "Read an existing file from the game project directory.",
        "input_schema": {
            "type": "object",
            "properties": {
                "relative_path": {
                    "type": "string",
                    "description": "Path relative to the game project root"
                }
            },
            "required": ["relative_path"]
        }
    },
    {
        "name": "list_files",
        "description": "List all generated files in the game project (or a subdirectory).",
        "input_schema": {
            "type": "object",
            "properties": {
                "sub_dir": {
                    "type": "string",
                    "description": "Optional subdirectory to list (e.g. 'backend'). Leave empty for all files."
                }
            },
            "required": []
        }
    },
    {
        "name": "append_to_design_doc",
        "description": "Add a section to the shared game-design.md document used by all agents.",
        "input_schema": {
            "type": "object",
            "properties": {
                "section": {
                    "type": "string",
                    "description": "Section heading (e.g. 'Game Mechanics', 'API Contracts')"
                },
                "content": {
                    "type": "string",
                    "description": "Markdown content for this section"
                }
            },
            "required": ["section", "content"]
        }
    },
    {
        "name": "read_design_doc",
        "description": "Read the shared game-design.md to understand decisions made by other agents.",
        "input_schema": {
            "type": "object",
            "properties": {},
            "required": []
        }
    }
]


def dispatch_file_tool(name: str, inputs: dict) -> str:
    """Execute a file tool by name and return JSON result string."""
    handlers = {
        "write_file": lambda i: write_file(i["relative_path"], i["content"]),
        "read_file": lambda i: read_file(i["relative_path"]),
        "list_files": lambda i: list_files(i.get("sub_dir", "")),
        "append_to_design_doc": lambda i: append_to_design_doc(i["section"], i["content"]),
        "read_design_doc": lambda i: read_design_doc(),
    }
    if name not in handlers:
        return json.dumps({"status": "error", "message": f"Unknown tool: {name}"})
    result = handlers[name](inputs)
    return json.dumps(result, indent=2)
