using MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Window;

public class DocumentHandler : ITextDocumentSyncHandler
{
  private readonly ILanguageServer _router;

  private readonly Container<TextDocumentFilter> _documentSelector = new Container<TextDocumentFilter>(
      new TextDocumentFilter
      {
        Pattern = "**/*.csproj"
      }
  );
  public DocumentHandler(ILanguageServer router)
  {
    _router = router;
  }

  public TextDocumentSyncKind Change { get; } = TextDocumentSyncKind.Full;

  public TextDocumentChangeRegistrationOptions GetRegistrationOptions()
  {
    return new TextDocumentChangeRegistrationOptions()
    {
      DocumentSelector = new TextDocumentSelector(_documentSelector),
      SyncKind = Change
    };
  }

  public TextDocumentChangeRegistrationOptions GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
  {
    throw new NotImplementedException();
  }

  public TextDocumentAttributes GetTextDocumentAttributes(Uri uri)
  {
    return new TextDocumentAttributes(uri, "xml");
  }

  public TextDocumentAttributes GetTextDocumentAttributes(DocumentUri uri)
  {
    throw new NotImplementedException();
  }

  public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
  {
    var documentPath = request.TextDocument.Uri.ToString();
    var text = request.ContentChanges.FirstOrDefault()?.Text;

    _router.Window.LogInfo($"Updated buffer for document: {documentPath}\n{text}");

    return Unit.Task;
  }

  public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
  {
    return Unit.Task;
  }

  public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
  {
    return Unit.Task;
  }

  public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
  {
    return Unit.Task;
  }

  TextDocumentOpenRegistrationOptions IRegistration<TextDocumentOpenRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
  {
    return new TextDocumentOpenRegistrationOptions()
    {
      DocumentSelector = new TextDocumentSelector(_documentSelector)
    };
  }

  TextDocumentCloseRegistrationOptions IRegistration<TextDocumentCloseRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
  {
    return new TextDocumentCloseRegistrationOptions()
    {
      DocumentSelector = new TextDocumentSelector(_documentSelector)
    };
  }

  TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions, TextSynchronizationCapability>.GetRegistrationOptions(TextSynchronizationCapability capability, ClientCapabilities clientCapabilities)
  {
    return new TextDocumentSaveRegistrationOptions()
    {
      DocumentSelector = new TextDocumentSelector(_documentSelector)
    };
  }
}
