using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using gRPCFullDuplex.Blazor.Application.Contract;
using gRPCFullDuplex.Blazor.Application.Handlers.Query;
using gRPCFullDuplex.Blazor.Client.Startup;
using gRPCFullDuplex.Shared.Contract;
using MediatR;

namespace gRPCFullDuplex.Blazor.Client.ViewModel;

public record PrimesVM(Factory<GenerateSetting, GenerateQuery> QueryFactory, IMediator Mediator, INotificationService NotificationService)
    : IDisposable, INotifyPropertyChanged
{
    public readonly int MaxRange = 10_000;
    public readonly int MaxDelay = 1000;

    public IComponentBase? View { get; internal set; }
    public int Range { get; set; } = 210;
    public int Speed { get; set; } = 500;

    public bool IsDisabled
    {
        get => _isDisabled;
        set => SetField(ref _isDisabled, value);
    }
    private bool _isDisabled = false;

    public int Delay => MaxDelay - Speed;
    public Wrap<int>[] SieveWebList { get; private set; } = Array.Empty<Wrap<int>>();
    public Wrap<int>[] SieveServerList { get; private set; } = Array.Empty<Wrap<int>>();
    public ConcurrentDictionary<int, Kind> Primes { get; } = new();
    public CancellationToken Token => CancellationTokenSource.Token;
    public CancellationTokenSource CancellationTokenSource
    {
        get => _cancellationTokenSource;
        private set
        {
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = value;
        }
    }
    private CancellationTokenSource _cancellationTokenSource = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public void OnInitialized() => NotificationService.PropertyChanged += OnPropertyChanged;
    
    /// <summary>
    /// Start and send query for generating prime numbers
    /// Channel is created to retrieve stream of prime numbers from Handler
    /// </summary>
    /// <returns></returns>
    public async Task Start()
    {
        try
        {
            var query = InitGenerate();
            var queryTask = Mediator.Send(query, Token);
            var readSieveChannelTask = ReadSieveChannel(query.SieveChannel);

            if (!await queryTask.ConfigureAwait(false) && !CancellationTokenSource.IsCancellationRequested)
                CancellationTokenSource.Cancel();

            await readSieveChannelTask.ConfigureAwait(false);
        }
        finally
        {
            Stop();
        }
    }
    public void Stop(bool isCancel = false)
    {
        if (isCancel && !CancellationTokenSource.IsCancellationRequested)
            CancellationTokenSource.Cancel();
        IsDisabled = false;
    }
    public void Cancel()
    {
        Stop(true);
        NotificationService.ErrorMessage = "Search has been cancelled.";
    }
    private GenerateQuery InitGenerate()
    {
        var settings = new GenerateSetting(Range);
        IsDisabled = true;
        Primes.Clear();
        SieveWebList = ToShowList(settings.Start, settings.WebRange).ToArray();
        SieveServerList = ToShowList(settings.ServerStart, settings.ServerRange).ToArray();
        CancellationTokenSource = new CancellationTokenSource();
        NotificationService.Clear();
        return QueryFactory(settings);
    }
    /// <summary>
    /// Prime numbers are read from the Channel as long as Query Handler is sending them
    /// </summary>
    /// <param name="sieveChannelReader"></param>
    /// <returns></returns>
    private async Task ReadSieveChannel(ISieveChannelReader sieveChannelReader)
    {
        await foreach (var number in sieveChannelReader.ChannelReader.ReadAllAsync(Token).ConfigureAwait(false))
        {
            if (Token.IsCancellationRequested) return;
            switch (number.Response)
            {
                case PrimeResponse prime:
                    Primes.TryAdd(prime.Value, number.Kind);
                    break;
                case NotPrimeResponse nonPrime:
                    if (number.Kind == Kind.Client) SieveWebList[nonPrime.Index].IsHidden = true;
                    if (number.Kind == Kind.Server) SieveServerList[nonPrime.Index].IsHidden = true;
                    break;
            }
            await View.StateHasChangedAsync().ConfigureAwait(false);
            await Task.Delay(Delay).ConfigureAwait(false);
        }
    }
    private static IEnumerable<Wrap<int>> ToShowList(int start, int range) =>
        Enumerable.Range(start, range).Select(x => new Wrap<int>(x, false));

    private async void OnPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (View != null) await View.StateHasChangedAsync().ConfigureAwait(false);
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null!;
    }
}

public record Wrap<T>(T Value, bool IsHidden)
{
    public T Value { get; set; } = Value;
    public bool IsHidden { get; set; } = IsHidden;
}