"""
BaseAgent — shared agentic loop for all specialists.
Each agent runs an autonomous tool-use loop using Claude Opus 4.6 with
adaptive thinking. Streaming is used to avoid HTTP timeouts on long outputs.
"""

import json
import anthropic
from config import MODEL, MAX_TOKENS, ANTHROPIC_API_KEY
from tools.file_tools import FILE_TOOL_SCHEMAS, dispatch_file_tool
from tools.game_tools import GAME_TOOL_SCHEMAS, dispatch_game_tool


class BaseAgent:
    """
    Base class for every specialist agent. Subclasses define:
      - self.name          : display name
      - self.agent_key     : key used in agent messages ("architect", "backend", …)
      - self.system_prompt : domain expertise injected as Claude's system prompt
      - self.extra_tools   : additional tool schemas specific to this agent
    """

    name: str = "Base Agent"
    agent_key: str = "base"
    system_prompt: str = "You are a helpful AI agent."
    extra_tools: list = []

    def __init__(self):
        self.client = anthropic.Anthropic(api_key=ANTHROPIC_API_KEY)
        self.messages: list[dict] = []

    # ------------------------------------------------------------------
    # Public API
    # ------------------------------------------------------------------

    def run(self, task: str, verbose: bool = True) -> str:
        """
        Run the agent on a task string. Returns the final text response.
        Uses an agentic loop: Claude → tool calls → results → Claude …
        until Claude produces an end_turn response.
        """
        if verbose:
            print(f"\n{'='*60}")
            print(f"  Agent: {self.name}")
            print(f"  Task : {task[:120]}{'...' if len(task) > 120 else ''}")
            print(f"{'='*60}")

        self.messages = [{"role": "user", "content": task}]
        all_tools = FILE_TOOL_SCHEMAS + GAME_TOOL_SCHEMAS + self.extra_tools

        final_text = ""
        while True:
            # Stream the response to avoid timeouts on large outputs
            response = self._stream_request(all_tools)
            final_text = self._extract_text(response)

            if response.stop_reason == "end_turn":
                break

            if response.stop_reason == "tool_use":
                tool_results = self._execute_tools(response, verbose)
                # Append assistant turn + tool results
                self.messages.append({"role": "assistant", "content": response.content})
                self.messages.append({"role": "user", "content": tool_results})
            else:
                # pause_turn or unexpected — just break
                break

        if verbose:
            print(f"\n[{self.name}] Done. Output preview:")
            print(final_text[:500] + ("..." if len(final_text) > 500 else ""))
        return final_text

    # ------------------------------------------------------------------
    # Internal helpers
    # ------------------------------------------------------------------

    def _stream_request(self, tools: list) -> anthropic.types.Message:
        """Stream a Messages API call and return the final Message object."""
        with self.client.messages.stream(
            model=MODEL,
            max_tokens=MAX_TOKENS,
            thinking={"type": "adaptive"},
            system=self.system_prompt,
            tools=tools,
            messages=self.messages,
        ) as stream:
            return stream.get_final_message()

    def _execute_tools(self, response: anthropic.types.Message, verbose: bool) -> list[dict]:
        """Execute all tool_use blocks in the response and return tool_result list."""
        results = []
        for block in response.content:
            if block.type != "tool_use":
                continue
            tool_name = block.name
            tool_input = block.input
            if verbose:
                print(f"  [tool] {tool_name}({json.dumps(tool_input)[:80]})")

            # Route to the right dispatcher
            if tool_name in {s["name"] for s in FILE_TOOL_SCHEMAS}:
                result_str = dispatch_file_tool(tool_name, tool_input)
            elif tool_name in {s["name"] for s in GAME_TOOL_SCHEMAS}:
                result_str = dispatch_game_tool(tool_name, tool_input)
            else:
                result_str = self._dispatch_extra_tool(tool_name, tool_input)

            results.append({
                "type": "tool_result",
                "tool_use_id": block.id,
                "content": result_str,
            })
        return results

    def _dispatch_extra_tool(self, name: str, inputs: dict) -> str:
        """Override in subclasses to handle agent-specific tools."""
        return json.dumps({"status": "error", "message": f"No handler for tool: {name}"})

    @staticmethod
    def _extract_text(response: anthropic.types.Message) -> str:
        return " ".join(
            block.text for block in response.content if block.type == "text"
        )
