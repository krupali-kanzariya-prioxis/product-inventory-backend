using System.ComponentModel;
using ModelContextProtocol.Server;

namespace ProductInventoryTrackerAPI.Mcp.Prompts;

[McpServerPromptType]
public sealed class ProductPrompts
{
    [McpServerPrompt(Name = "weekly_inventory_review")]
    [Description("Use this prompt when the user asks for a weekly inventory review, a summary of stock health, or a suggested product follow-up plan.")]
    public string WeeklyInventoryReview(
        [Description("The week start date in ISO format, used to anchor the review narrative.")] DateTime weekStartDate)
    {
        return $$"""
You are reviewing the product inventory week that starts on {{weekStartDate:yyyy-MM-dd}}.

Workflow:
1. Read the resource app://products/summary first.
2. If low-stock products need more detail, call the tool search_products with lowStockOnly=true.
3. If a specific product needs inspection, read the resource app://products/{productSid}.
4. Summarize the current inventory health, highlight low-stock items, and suggest a concise follow-up plan.
5. If you recommend updating a product, call get_product first to confirm the current values and only send the fields that need to change.
6. Ask for confirmation before calling any write tool such as create_product or update_product.

Keep the final answer concise and explicitly separate observations from proposed actions.
""";
    }

    [McpServerPrompt(Name = "low_stock_triage")]
    [Description("Use this prompt when the user wants help triaging low-stock products or deciding what inventory items need attention first.")]
    public string LowStockTriage(
        [Description("The maximum number of low-stock products to inspect in detail.")] int limit = 10)
    {
        return $$"""
Triage the current low-stock inventory situation.

Workflow:
1. Read app://products/summary.
2. Call search_products with lowStockOnly=true and limit={{limit}}.
3. Group the results by urgency using current stock versus reorder threshold.
4. If you need deeper context for any one item, read app://products/{productSid}.
5. Before recommending update_product, call get_product for the chosen item and only send the fields that need to change.
6. Recommend the next actions, but ask for confirmation before using create_product or update_product.

Do not guess product SIDs. Look them up first and keep the response concise.
""";
    }
}
