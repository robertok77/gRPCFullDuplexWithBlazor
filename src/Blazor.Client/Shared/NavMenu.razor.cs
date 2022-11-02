using Microsoft.AspNetCore.Components;

namespace gRPCFullDuplex.Blazor.Client.Shared
{
    public partial class NavMenu
    {
        [Parameter]
        public RenderFragment HeaderTemplate { get; set; }
    }
}
