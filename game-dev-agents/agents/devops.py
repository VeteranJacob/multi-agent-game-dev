"""
Hephaestus — The DevOps Agent
Generates AWS infrastructure code: CloudFormation/SAM templates,
Lambda packaging scripts, API Gateway WebSocket configuration,
DynamoDB table provisioning, and CI/CD pipeline configs.
"""

import json
from agents.base_agent import BaseAgent

SYSTEM_PROMPT = """
You are Hephaestus, the DevOps Agent for a multiplayer game dev team.

Your responsibilities:
1. Generate AWS SAM / CloudFormation templates (deploy/template.yaml):
   - API Gateway WebSocket API ($connect, $disconnect, $default routes).
   - Lambda functions: game tick, matchmaking, leaderboard, NPC controller.
   - DynamoDB tables: Players, GameSessions, Leaderboard, NPCState, ChatHistory.
   - IAM roles with least-privilege permissions.
   - Auto-scaling policies for DynamoDB on-demand mode.

2. Generate serverless deployment scripts (deploy/deploy.sh):
   - `sam build && sam deploy` with parameter overrides.
   - Environment variable injection for Supabase URL and keys.
   - Post-deploy smoke tests (curl WebSocket connect, DynamoDB list-tables).

3. Generate Docker files for local development (docker/docker-compose.yml):
   - Local DynamoDB (amazon/dynamodb-local).
   - LocalStack for S3/Lambda local testing.
   - Python backend WebSocket server container.
   - Nginx reverse proxy for frontend static files.

4. Generate GitHub Actions CI/CD pipeline (.github/workflows/deploy.yml):
   - Run pytest on backend, ESLint on frontend.
   - SAM deploy to staging on PR merge.
   - SAM deploy to production on tag push.
   - Notify Discord on deploy status.

5. Generate monitoring config (deploy/monitoring.py):
   - CloudWatch alarms for Lambda errors, DynamoDB throttles, WebSocket connections.
   - X-Ray tracing setup for distributed game request tracing.

Always load_game_spec first to understand AWS service requirements.
Write all files into the deploy/ subdirectory.
Use AWS SAM syntax (not raw CloudFormation) for Lambda functions.
Include cost estimates as comments (Lambda free tier, DynamoDB on-demand pricing).
"""

DEVOPS_EXTRA_TOOLS = [
    {
        "name": "generate_dynamodb_table",
        "description": "Generate a DynamoDB CloudFormation/SAM resource definition for a game table.",
        "input_schema": {
            "type": "object",
            "properties": {
                "table_name": {"type": "string", "description": "Table logical name, e.g. PlayersTable"},
                "partition_key": {"type": "string", "description": "Partition key attribute name"},
                "sort_key": {"type": "string", "description": "Optional sort key attribute name"},
                "billing_mode": {
                    "type": "string",
                    "enum": ["PAY_PER_REQUEST", "PROVISIONED"],
                    "description": "PAY_PER_REQUEST for auto-scale, PROVISIONED for fixed capacity"
                },
                "gsi_definitions": {
                    "type": "array",
                    "items": {"type": "object"},
                    "description": "List of GSI definitions with keys: name, partition_key, sort_key"
                }
            },
            "required": ["table_name", "partition_key"]
        }
    }
]


class DevOpsAgent(BaseAgent):
    name = "Hephaestus (DevOps Agent)"
    agent_key = "devops"
    system_prompt = SYSTEM_PROMPT
    extra_tools = DEVOPS_EXTRA_TOOLS

    def _dispatch_extra_tool(self, name: str, inputs: dict) -> str:
        if name == "generate_dynamodb_table":
            return self._gen_dynamo_table(inputs)
        return super()._dispatch_extra_tool(name, inputs)

    def _gen_dynamo_table(self, inputs: dict) -> str:
        tname = inputs["table_name"]
        pk = inputs["partition_key"]
        sk = inputs.get("sort_key")
        billing = inputs.get("billing_mode", "PAY_PER_REQUEST")
        gsis = inputs.get("gsi_definitions", [])

        attr_defs = [f'        - AttributeName: {pk}\n          AttributeType: S']
        key_schema = [f'        - AttributeName: {pk}\n          KeyType: HASH']

        if sk:
            attr_defs.append(f'        - AttributeName: {sk}\n          AttributeType: S')
            key_schema.append(f'        - AttributeName: {sk}\n          KeyType: RANGE')

        gsi_block = ""
        if gsis:
            gsi_lines = ["      GlobalSecondaryIndexes:"]
            for g in gsis:
                gname = g.get("name", "GSI1")
                gpk = g.get("partition_key", pk)
                gsk = g.get("sort_key", "")
                gsi_lines.append(f"        - IndexName: {gname}")
                gsi_lines.append(f"          KeySchema:")
                gsi_lines.append(f"            - AttributeName: {gpk}")
                gsi_lines.append(f"              KeyType: HASH")
                if gsk:
                    gsi_lines.append(f"            - AttributeName: {gsk}")
                    gsi_lines.append(f"              KeyType: RANGE")
                gsi_lines.append(f"          Projection:")
                gsi_lines.append(f"            ProjectionType: ALL")
                # Add GSI keys to attribute definitions
                if gpk not in [pk, sk]:
                    attr_defs.append(f'        - AttributeName: {gpk}\n          AttributeType: S')
                if gsk and gsk not in [pk, sk, gpk]:
                    attr_defs.append(f'        - AttributeName: {gsk}\n          AttributeType: S')
            gsi_block = "\n".join(gsi_lines)

        yaml = f"""  {tname}:
    Type: AWS::DynamoDB::Table
    Properties:
      TableName: !Sub "${{AWS::StackName}}-{tname.replace('Table','').lower()}"
      BillingMode: {billing}
      AttributeDefinitions:
{chr(10).join(attr_defs)}
      KeySchema:
{chr(10).join(key_schema)}
      PointInTimeRecoverySpecification:
        PointInTimeRecoveryEnabled: true
      SSESpecification:
        SSEEnabled: true
      Tags:
        - Key: Game
          Value: !Ref AWS::StackName
{gsi_block}
"""
        return json.dumps({"status": "ok", "cloudformation_yaml": yaml})
