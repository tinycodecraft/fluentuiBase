using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;



/// <summary>
/// Strongly-typed representation of:
///   https://www.ag-grid.com/javascript-grid-column-properties/
/// </summary>
public partial class ColumnDefinition
{
    public string Field { get; set; }

    public string HeaderName { get; set; }

    [JsonPropertyName("resizable")]
    public bool IsResizable { get; set; }

    [JsonPropertyName("sortable")]
    public bool IsSortable { get; set; }

    [JsonPropertyName("filter")]
    public object Filter { get; set; }

    [JsonPropertyName("editable")]
    public bool IsEditable { get; set; }

    [JsonPropertyName("floatingFilter")]
    public bool HasFloatingFilter { get; set; }
    [JsonPropertyName("hide")]
    public bool Suppress { get; set; }
    [JsonPropertyName("suppressToolPanel")]
    public bool NoToolPanel { get; set; }
    [JsonPropertyName("choices")]
    public string[] Choices { get; set; }
}
