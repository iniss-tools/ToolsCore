using System.Reflection;

namespace ToolsCore.Mcp;

/// <summary>
/// Server MCP programu: nazov, pokyny pre agenta pri pripojeni a zostava s nastrojmi.
/// </summary>
/// <param name="Name">nazov servera pre klienta (napr. <c>GVDEditor</c>)</param>
/// <param name="Instructions">pokyny pre agenta - co program je a ako nastroje pouzivat</param>
/// <param name="ReadOnlyInstructions">pokyny navyse, ked agent smie len citat</param>
/// <param name="WriteInstructions">pokyny navyse, ked agent smie menit data</param>
/// <param name="ToolAssembly">zostava so statickymi nastrojmi v triedach <c>[McpServerToolType]</c></param>
public sealed record McpServerDefinition(string Name, string Instructions, string ReadOnlyInstructions, string WriteInstructions, Assembly ToolAssembly);
