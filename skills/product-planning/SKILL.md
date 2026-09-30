---
name: product-planning
description: Use when the user asks to review, prioritize, plan, summarize, or triage product inventory items in this app.
---

# Product Planning Skill

Use this skill to reason about product inventory with the Product Inventory Tracker MCP server.

## Workflow
1. Read the current summary resource at `app://products/summary` to understand total inventory, low-stock counts, and category distribution.
2. Call `search_products` when you need a concise list of matching products, especially when the user asks about low-stock inventory or when you need to look up product SIDs before any other action.
3. Read `app://products/{productSid}` for any specific product that needs deeper inspection or recent transaction context.
4. Group the findings into a concise plan with observations, priorities, and proposed next actions.
5. Ask the user to confirm before calling any write tool, including `create_product` or `update_product`.
6. After confirmation, execute only the requested changes and report the results using `templates/report-format.md`.

## Rules
- Never delete anything.
- Never guess product SIDs; look them up first.
- Before `update_product`, call `get_product` to confirm the current values, and only send the fields that need to change.
- Keep output concise and action-oriented.
- Say exactly what changed after any write tool call.
- Prefer read-only resources and tools before write actions.
- If a requested product does not exist, say so clearly and ask for the next step.
