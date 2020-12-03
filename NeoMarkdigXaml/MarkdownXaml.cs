#region -- copyright --
//
// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
//
#endregion
using System;
using System.IO;
using System.Windows.Documents;
using System.Xaml;
using Markdig;
using Neo.Markdig.Xaml.Renderers;

namespace Neo.Markdig.Xaml
{
	public static partial class MarkdownXaml
	{
		/// <summary>Converts a Markdown string to a FlowDocument.</summary>
		/// <param name="markdown">A Markdown text.</param>
		/// <param name="pipeline">The pipeline used for the conversion.</param>
		/// <param name="baseUri">Base uri for images and links.</param>
		/// <returns>The result of the conversion</returns>
		/// <exception cref="System.ArgumentNullException">if markdown variable is null</exception>
		public static FlowDocument ToFlowDocument(string markdown, MarkdownPipeline pipeline = null, Uri baseUri = null)
		{
			if (markdown == null)
				throw new ArgumentNullException(nameof(markdown));
			if (pipeline == null)
				pipeline = new MarkdownPipelineBuilder().Build();

			using (var writer = new XamlObjectWriter(System.Windows.Markup.XamlReader.GetWpfSchemaContext()))
				return (FlowDocument)ToXaml(markdown, writer, pipeline, baseUri);
		} // func ToFlowDocument

		/// <summary>Converts a Markdown string to XAML.</summary>
		/// <param name="markdown">A Markdown text.</param>
		/// <param name="pipeline">The pipeline used for the conversion.</param>
		/// <param name="baseUri">Base uri for images and links.</param>
		/// <returns>The result of the conversion</returns>
		/// <exception cref="ArgumentNullException">if markdown variable is null</exception>
		public static string ToXaml(string markdown, MarkdownPipeline pipeline = null, Uri baseUri = null)
		{
			if (markdown == null)
				throw new ArgumentNullException(nameof(markdown));

			using (var writer = new StringWriter())
			{
				ToXaml(markdown, writer, pipeline, baseUri);
				return writer.ToString();
			}
		} // func ToXaml

		/// <summary>Converts a Markdown string to XAML and output to the specified writer.</summary>
		/// <param name="markdown">A Markdown text.</param>
		/// <param name="writer">The destination <see cref="TextWriter"/> that will receive the result of the conversion.</param>
		/// <param name="pipeline">The pipeline used for the conversion.</param>
		/// <param name="baseUri">Base uri for images and links.</param>
		public static void ToXaml(string markdown, TextWriter writer, MarkdownPipeline pipeline = null, Uri baseUri = null)
		{
			if (markdown == null)
				throw new ArgumentNullException(nameof(markdown));
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));

			using (var xamlWriter = new XamlXmlWriter(writer, System.Windows.Markup.XamlReader.GetWpfSchemaContext(), new XamlXmlWriterSettings() { CloseOutput = false }))
			{
				ToXaml(markdown, xamlWriter, pipeline, baseUri);
				xamlWriter.Flush();
			}
		} // func ToXaml


		/// <summary>Converts a Markdown string to XAML and output to the specified writer.</summary>
		/// <param name="markdown">A Markdown text.</param>
		/// <param name="writer">The destination <see cref="TextWriter"/> that will receive the result of the conversion.</param>
		/// <param name="pipeline">The pipeline used for the conversion.</param>
		/// <param name="baseUri">Base uri for images and links.</param>
		/// <returns></returns>
		public static object ToXaml(string markdown, XamlWriter writer, MarkdownPipeline pipeline = null, Uri baseUri = null)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			pipeline = pipeline ?? new MarkdownPipelineBuilder().Build();

			var renderer = new XamlMarkdownWriter(writer) { BaseUri = baseUri };
			pipeline.Setup(renderer);

			var document = Markdown.Parse(markdown, pipeline);
			return renderer.Render(document);
		} // proc ToXaml

		public static MarkdownPipelineBuilder UseXamlSupportedExtensions(this MarkdownPipelineBuilder pipeline)
		{
			if (pipeline == null) throw new ArgumentNullException(nameof(pipeline));
			return pipeline
				.UseEmphasisExtras()
				.UseGridTables()
				.UsePipeTables()
				.UseTaskLists()
				.UseAutoLinks();
		} // func UseXamlSupportedExtensions
	} // class MarkdownXaml
}
