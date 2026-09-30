param(
	[string]$Url = 'http://localhost:5105/mcp'
)

$ErrorActionPreference = 'Stop'

function ConvertFrom-McpResponseContent {
	param([string]$Content)

	$trimmed = $Content.Trim()
	if ($trimmed.StartsWith('{') -or $trimmed.StartsWith('[')) {
		return $trimmed | ConvertFrom-Json -Depth 20
	}

	$dataLines = @()
	foreach ($line in ($Content -split "`r?`n")) {
		if ($line.StartsWith('data:')) {
			$dataLines += $line.Substring(5).Trim()
		}
	}

	if ($dataLines.Count -eq 0) {
		throw "The MCP response was not JSON and did not contain SSE data lines that could be parsed."
	}

	$json = ($dataLines -join "`n").Trim()
	return $json | ConvertFrom-Json -Depth 20
}

function Invoke-McpRequest {
	param(
		[string]$RequestBody,
		[string]$SessionId
	)

	$headers = @{ Accept = 'application/json, text/event-stream' }
	if ($SessionId) {
		$headers['Mcp-Session-Id'] = $SessionId
	}

	$response = Invoke-WebRequest -Uri $Url -Method Post -Headers $headers -ContentType 'application/json' -Body $RequestBody
	$parsed = ConvertFrom-McpResponseContent -Content $response.Content

	$newSessionId = $null
	if ($response.Headers['Mcp-Session-Id']) {
		$newSessionId = $response.Headers['Mcp-Session-Id']
	}

	[PSCustomObject]@{
		Parsed = $parsed
		SessionId = if ($newSessionId) { $newSessionId } else { $SessionId }
		ContentType = $response.Headers['Content-Type']
	}
}

$initializeBody = @'
{
  "jsonrpc": "2.0",
  "id": "init-1",
  "method": "initialize",
  "params": {
	"protocolVersion": "2025-11-05",
	"capabilities": {},
	"clientInfo": {
	  "name": "powershell-smoke-test",
	  "version": "1.0.0"
	}
  }
}
'@

$initializeResponse = Invoke-McpRequest -RequestBody $initializeBody -SessionId $null
$sessionId = $initializeResponse.SessionId
Write-Host "Initialized MCP session. SessionId: $sessionId"

$initializedBody = @'
{
  "jsonrpc": "2.0",
  "method": "notifications/initialized",
  "params": {}
}
'@

[void](Invoke-McpRequest -RequestBody $initializedBody -SessionId $sessionId)

$listToolsBody = @'
{
  "jsonrpc": "2.0",
  "id": "tools-1",
  "method": "tools/list",
  "params": {}
}
'@

$listToolsResponse = Invoke-McpRequest -RequestBody $listToolsBody -SessionId $sessionId
$toolNames = @($listToolsResponse.Parsed.result.tools | ForEach-Object { $_.name })

Write-Host 'Discovered tools:'
$toolNames | ForEach-Object { Write-Host "- $_" }
