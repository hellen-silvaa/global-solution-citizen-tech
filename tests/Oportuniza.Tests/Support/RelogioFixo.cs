namespace Oportuniza.Tests.Support;

/// <summary>Relógio controlado: os testes não dependem da data em que rodam.</summary>
public class RelogioFixo(DateTime agoraUtc) : TimeProvider
{
    public DateTime Agora { get; set; } = agoraUtc;

    public override DateTimeOffset GetUtcNow() => new(Agora, TimeSpan.Zero);
}
