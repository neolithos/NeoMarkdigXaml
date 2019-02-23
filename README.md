Neo.Markdig.Xaml
================

Is a xaml/wpf extension for [lunet-io/markdig](https://github.com/lunet-io/markdig)

With this library it is possible to create FlowDocument's 

```C#
var content = File.ReadAllText(@"..\..\..\..\Readme.md");
var doc = MarkdownXaml.ToFlowDocument(content,
	new MarkdownPipelineBuilder()
	.UseXamlSupportedExtensions()
	.Build()
);
flowDocumentViewer.Document = doc;
```

or Xaml-Source files.

```C#
var content = File.ReadAllText(@"..\..\..\..\Readme.md");
var xaml = MarkdownXaml.ToXaml(content,
	new MarkdownPipelineBuilder()
	.UseXamlSupportedExtensions()
	.Build()
);
```

## Features

Supports standard features from Markdig.

Additionally, the following extensions are supported:
- AutoLinks
- Tables
- Extra emphasis

## Styles

It is possible to replace the default resource set.

Inherit from `XamlMarkdownWriter` and  override the method `GetDefaultStyle`.

## Difference to [markdig.wpf](https://github.com/Kryptos-FR/markdig.wpf)

markdig.wpf implements **two** renderers, one for xaml-output and one for wpf-objects.

This implementation has only **one** renderer, that has a xaml-token-stream as output.
This stream can written with framework implemented `XamlXmlWriter` or the `XamlObjectWriter`.

Numbers:
```
Markdig.Wpf-toxaml  : 109ms
Markdig.Xaml-toxaml : 850ms
Markdig.Wpf-towpf   : 12.579ms
Markdig.Xaml-towpf  : 6.882ms
```

If you want to create plain xaml text output, my library is a slower. The reason is, that my implementation 
validates the output against the wpf-object-schema. That is one expensive step more, but it ensures a 
valid wpf-xaml.

The object creation should be always faster in Neo.Markdig.Xaml than the Markdig.Wpf approach, because
Neo.Markdig.Xaml uses the internal wpf-object creation with all is caching and replay mechanism.

Needs to prove: Neo.Markdig.Xaml should consume less memory on large documents in both cases.

## Links
- Nuget [Neo.Markdig.Xaml](https://www.nuget.org/packages/Neo.Markdig.Xaml/)
- [TecWare GmbH](https://www.tecware-gmbh.de/)

## What is missing

Testing.