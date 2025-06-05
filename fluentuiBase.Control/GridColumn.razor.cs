using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fluentuiBase.Control
{
    public partial class GridColumn : ComponentBase
    {
        [CascadingParameter(Name = nameof(GridOptions.ColumnDefinitions))]
        public List<ColumnDefinition> ColumnDefinitions { get; set; }

        [Parameter] public string Field { get; set; }
        [Parameter] public string Header { get; set; }
        [Parameter] public bool IsResizable { get; set; }
        [Parameter] public bool IsSortable { get; set; }
        
        [Parameter] public bool IsEditable { get; set; }

        [Parameter] public string Filter { get; set; }

        [Parameter] public bool HideFilter { get; set; }

        [Parameter] public bool ToSuppress { get; set; }

        [Parameter] public string Choices { get; set; }

        [Parameter] public string Urlis { get; set; }
        [Parameter] public string Wanted { get; set; }

        private object getFilter(string filtertype) => filtertype switch
        {
            "string" => "agTextColumnFilter",
            "number" => "agNumberColumnFilter",
            _ => false
        };
        static string[] validfilters =new [] { "string", "number" };

        protected override void OnInitialized()
        {
            ColumnDefinitions.Add(new ColumnDefinition
            {
                Field = Field,
                HeaderName = Header,
                IsResizable = IsResizable,
                IsSortable = IsSortable,
                Filter = getFilter(Filter),
                IsEditable = IsEditable,
                HasFloatingFilter = !HideFilter && validfilters.Contains(Filter),
                NoToolPanel = ToSuppress,
                Suppress = ToSuppress,
                Choices = Choices == null ? new string[] { }: Choices.Split('|').Select(c => c.Trim()).ToArray(),
                Wanted = Wanted,
                Urlis = Urlis
            }); ;
        }
    }
}
