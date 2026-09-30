# MCP Testing Guide

## Run the API
From `ProductInventoryTrackerAPI/`:

- `dotnet run`

Expected local URLs from `launchSettings.json`:
- HTTP: `http://localhost:5105`
- HTTPS: `https://localhost:7048`
- MCP endpoint: `http://localhost:5105/mcp`

## Inspector steps
1. Start the API.
2. Run `npx @modelcontextprotocol/inspector`.
3. Choose **Streamable HTTP**.
4. Use `http://localhost:5105/mcp`.
5. Verify the server lists tools, resources, and prompts.
6. Call the read-only tools/resources first.
7. Confirm any write action before testing `create_product` or `update_product`.

## Tool test cases
| Surface | Positive test | Negative test |
|---|---|---|
| `search_products` | Search with `limit=5` and verify matching items are returned | Use `limit=0` or `limit=99` and verify an McpException-style validation message is returned |
| `get_product` | Use a real `productSid` from `search_products` and verify `description`, `categorySid`, and `supplierSid` are present | Send a blank or fake SID and verify the response says to use `search_products` |
| `create_product` | Create a valid product and verify the returned item includes the new SID and the supplied values | Send `unitPrice=0` or a blank `productName` and verify a human-readable validation message |
| `update_product` | Call `get_product`, then update only one field such as `currentStock` and verify other fields remain unchanged | Call with only `productSid` and no other fields, or with a blank SID, and verify an McpException-style validation message |
| `get_inventory_summary` | Verify counts and category groups are returned | N/A - this tool has no required input |

## Resource test cases
| Resource | Positive test | Negative test |
|---|---|---|
| `app://products/summary` | Read the resource and verify formatted JSON is returned | N/A |
| `app://products/{productSid}` | Read with a real SID and verify product details plus recent transactions are returned as formatted JSON | Read with a fake SID and verify the response says to use `search_products` |
| `app://schema` | Read and verify the plain-text domain rules are returned | N/A |

## Prompt test cases
| Prompt | Positive test | Negative test |
|---|---|---|
| `weekly_inventory_review` | Generate the prompt and verify it tells the model to read resources first and confirm writes | Use an obviously invalid date and verify the MCP client rejects or normalizes it before execution |
| `low_stock_triage` | Generate the prompt and verify it tells the model to call `search_products`, then `get_product` before `update_product` | Use an unusually large limit and verify the prompt still only describes workflow, not direct execution |

## Claude Code connect and Skill install
### Connect Claude Code
- `claude mcp add --transport http product-inventory http://localhost:5105/mcp`

### Install the Skill locally
From the repo root:
- `powershell -ExecutionPolicy Bypass -File .\scripts\install-skill.ps1`

This copies `skills/product-planning` into `.claude/skills/product-planning`.

## With-Skill vs without-Skill comparison
1. Connect Claude Code to the MCP server.
2. Ask for a low-stock review without installing the Skill and note the workflow and quality of the answer.
3. Install the Skill with `scripts/install-skill.ps1`.
4. Ask the same question again.
5. Verify the Skill-enabled flow is more explicit about:
   - reading `app://products/summary` first
   - looking up SIDs before updates
   - calling `get_product` before `update_product`
   - only sending changed fields in `update_product`

## Optional smoke test script
If the API is already running:
- `powershell -ExecutionPolicy Bypass -File .\scripts\smoke-test.ps1`

The script sends:
1. `initialize`
2. `notifications/initialized`
3. `tools/list`

Then it prints the discovered tool names.

## Troubleshooting
| Problem | What to check |
|---|---|
| Wrong URL or port | Verify the API is running on `http://localhost:5105` and use `http://localhost:5105/mcp` in Inspector and Claude Code |
| HTTP vs HTTPS mismatch | If HTTPS is enforced in your environment, try `https://localhost:7048/mcp` instead |
| Stale tool list | Restart the API and reconnect the MCP client or Inspector session |
| Skill not detected | Confirm `.claude/skills/product-planning/SKILL.md` exists after running `install-skill.ps1` |
| Session errors in smoke test | Make sure the API is running and that the returned `Mcp-Session-Id` header is being preserved |
| Tool validation errors | Use `search_products` to find a valid SID, then call `get_product` before `update_product` |
