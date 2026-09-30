# MCP in Product Inventory Tracker

## What MCP is
Model Context Protocol (MCP) is a standard way for an AI host to discover and use application capabilities.
A **host** is the AI application, such as Claude Code or the MCP Inspector.
A **client** is the connector inside that host that talks to a server.
A **server** is this ASP.NET Core API endpoint at `/mcp`.
**Tools** perform actions, **resources** provide read-only context, and **prompts** provide reusable instructions.
Agent **Skills** are different: Skills teach workflow and judgment, while MCP provides direct access to data and actions.

## Architecture
```mermaid
flowchart LR
	FE[Next.js Frontend] --> REST[REST Controllers]
	REST --> SVC[Repositories / Shared Services]
	SVC --> DB[(SQL Server / LocalDB)]

	Claude[MCP Client / Claude] --> MCP[/mcp endpoint]
	MCP --> SVC
```

## Project summary
- Domain chosen for the demo: **Products**
- Existing REST API behavior was kept unchanged.
- The MCP server is hosted inside the existing .NET 10 Web API.
- The MCP HTTP endpoint is mapped at `/mcp`.
- The current app does not configure authentication, so the MCP endpoint is open for local demo use just like the rest of the API.
- The installed SDK supports HTTP transport configuration, including a stateless option, but this demo keeps the default behavior.

## MCP tools
| Name | Purpose | Mode |
|---|---|---|
| `search_products` | Search active products or list low-stock products with a capped result set | Read-only |
| `get_product` | Get one product by product SID | Read-only |
| `create_product` | Create a new product after user confirmation | Write |
| `update_product` | Update an existing product after user confirmation | Write |
| `get_inventory_summary` | Get a concise inventory summary with counts and low-stock totals | Read-only |

## MCP resources
| URI / Template | Purpose | Mode |
|---|---|---|
| `app://products/summary` | Current inventory summary and counts by category | Read-only |
| `app://products/{productSid}` | Product detail plus recent stock transactions | Read-only |
| `app://schema` | Plain-text description of the product inventory model | Read-only |

## MCP prompts
| Name | Purpose | Mode |
|---|---|---|
| `weekly_inventory_review` | Guide a weekly inventory review using resources first | Read-only |
| `low_stock_triage` | Guide low-stock triage and prioritization | Read-only |

## Agent Skill
The repo also includes a separate Agent Skill at `skills/product-planning/`.

- Claude Code: copy `skills/product-planning` into `.claude/skills/` in the project or into `~/.claude/skills/`.
- Claude.ai / Claude Desktop: zip the `skills/product-planning` folder and upload it in **Settings > Capabilities**. Verify the exact upload steps in the latest Anthropic documentation.

## Run and verify
### Run the API
From the API project folder:

`dotnet run`

Launch profiles currently expose:
- HTTP: `http://localhost:5105`
- HTTPS: `https://localhost:7048`

### Inspect with MCP Inspector
1. Run: `npx @modelcontextprotocol/inspector`
2. Choose transport: **Streamable HTTP**
3. Use URL: `http://localhost:5105/mcp`
4. Verify that tools, resources, and prompts are listed
5. Call each item to confirm the server can read and write through the existing product logic

### Connect Claude Code
`claude mcp add --transport http product-inventory http://localhost:5105/mcp`

## Demo script
1. Show the existing REST API or frontend still working.
2. Open MCP Inspector and show discovery of tools, resources, and prompts.
3. Ask Claude: “what’s the status of my products?” and let it read `app://products/summary`.
4. Ask Claude to triage low-stock products using the Skill and MCP tools.
5. If you confirm a write, let Claude call `create_product` or `update_product`.
6. Refresh the Next.js frontend to show the same data change through the existing UI.
7. Compare the result with and without the `product-planning` Skill enabled.

## Security notes
- The `/mcp` endpoint exposes only a small, least-privilege surface over the Product domain.
- Delete and admin operations are intentionally not exposed as MCP tools.
- Prompts and Skill instructions tell the model to ask for confirmation before write actions.
- CORS is not required for MCP unless a browser-based client calls `/mcp` directly.
- Do not place secrets or connection strings in Skill files or MCP prompt content.
