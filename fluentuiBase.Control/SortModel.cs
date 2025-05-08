using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;



public class SortModel
{
    [JsonPropertyName("colId")]
    public string ColumnId { get; set; }

    [JsonPropertyName("sort")]
    public string Direction { get; set; }
}
