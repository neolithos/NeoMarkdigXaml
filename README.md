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
