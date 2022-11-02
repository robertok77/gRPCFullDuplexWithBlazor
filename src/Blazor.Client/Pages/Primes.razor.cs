
using gRPCFullDuplex.Blazor.Application.Contract;
using gRPCFullDuplex.Blazor.Client.Startup;
using gRPCFullDuplex.Blazor.Client.ViewModel;
using Microsoft.AspNetCore.Components;

namespace gRPCFullDuplex.Blazor.Client.Pages;

public partial class Primes : ComponentBase, IComponentBase, IDisposable
{
    [Inject] public PrimesVM ViewModel { get; internal set; } = null!;
    
    protected override void OnInitialized()
    {
        ViewModel.View = this;
        ViewModel.OnInitialized();
    }

    public Task StateHasChangedAsync() => InvokeAsync(StateHasChanged);

    public void Dispose() => ViewModel?.Dispose();
}
