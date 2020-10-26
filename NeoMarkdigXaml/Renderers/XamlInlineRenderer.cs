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
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xaml;
using Markdig.Syntax.Inlines;

namespace Neo.Markdig.Xaml.Renderers.Inlines
{
	#region -- class AutolinkInlineRenderer -------------------------------------------

	public class AutolinkInlineRenderer : XamlObjectRenderer<AutolinkInline>
	{
		protected override void Write(XamlMarkdownWriter renderer, AutolinkInline link)
		{
			var url = link.Url;
			var title = url;

			if (link.IsEmail)
				url = "mailto:" + url;

			LinkInlineRenderer.WriteStartHyperlink(renderer, url, title);
			renderer.WriteItems(title);
			LinkInlineRenderer.WriteEndHyperlink(renderer);
		} // proc Write
	} // class AutolinkInlineRenderer

	#endregion

	#region -- class CodeInlineRenderer -----------------------------------------------

	public class CodeInlineRenderer : XamlObjectRenderer<CodeInline>
	{
		protected override void Write(XamlMarkdownWriter renderer, CodeInline code)
		{
			renderer.WriteStartObject(typeof(Span));
			renderer.WriteResourceMember(null, MarkdownXamlStyle.Code);

			renderer.WriteStartItems(nameof(Span.Inlines), true);
			renderer.WriteText(code.Content);
			renderer.WriteEndItems();
			renderer.WriteEndObject();
		} // proc Write
	} // proc class CodeInlineRenderer

	#endregion

	#region -- class DelimiterInlineRenderer ------------------------------------------

	public class DelimiterInlineRenderer : XamlObjectRenderer<DelimiterInline>
	{
		protected override void Write(XamlMarkdownWriter renderer, DelimiterInline obj)
		{
			renderer.WriteText(obj.ToLiteral());
			renderer.WriteChildren(obj);
		} // proc Write
	} // class DelimiterInlineRenderer

	#endregion

	#region -- class EmphasisInlineRenderer -------------------------------------------

	public class EmphasisInlineRenderer : XamlObjectRenderer<EmphasisInline>
	{
		private static bool WriteSpan(XamlMarkdownWriter renderer, EmphasisInline span)
		{
			switch (span.DelimiterChar)
			{
				case '*' when span.DelimiterCount == 2: // bold
				case '_' when span.DelimiterCount == 2: // bold
					renderer.WriteStartObject(typeof(Bold));
					return true;
				case '*': // italic
				case '_': // italic
					renderer.WriteStartObject(typeof(Italic));
					return true;
				case '~': // strike through
					renderer.WriteStartObject(typeof(Span));
					renderer.WriteResourceMember(null, MarkdownXamlStyle.StrikeThrough);
					return true;
				case '^': // superscript, subscript
					renderer.WriteStartObject(typeof(Span));
					if (span.DelimiterCount == 2)
						renderer.WriteResourceMember(null, MarkdownXamlStyle.Superscript);
					else
						renderer.WriteResourceMember(null, MarkdownXamlStyle.Subscript);
					return true;
				case '+': // underline
					renderer.WriteStartObject(typeof(Span));
					renderer.WriteResourceMember(null, MarkdownXamlStyle.Inserted);
					return true;
				case '=': // Marked
					renderer.WriteStartObject(typeof(Span));
					renderer.WriteResourceMember(null, MarkdownXamlStyle.Marked);
					return true;
				default:
					return false;
			}
		} // proc WriteSpan

		protected override void Write(XamlMarkdownWriter renderer, EmphasisInline span)
		{
			if (WriteSpan(renderer, span))
			{
				renderer.WriteItems(span);
				renderer.WriteEndObject();
			}
			else
				renderer.WriteChildren(span);
		} // proc Write
	} // class EmphasisInlineRenderer 

	#endregion

	#region -- class EntityInlineRenderer ---------------------------------------------

	public class EntityInlineRenderer : XamlObjectRenderer<HtmlEntityInline>
	{
		protected override void Write(XamlMarkdownWriter renderer, HtmlEntityInline obj)
		{
			var txt = obj.Transcoded.Text.Substring(obj.Transcoded.Start, obj.Transcoded.Length);
			using (var xaml = new XamlXmlReader(new StringReader(txt), new XamlXmlReaderSettings() { }))
			{
				while (xaml.Read())
				{
					switch (xaml.NodeType)
					{
						case XamlNodeType.NamespaceDeclaration:
							renderer.WriteNamespace(xaml.Namespace);
							break;
						case XamlNodeType.StartObject:
							renderer.WriteStartObject(xaml.Type);
							break;
						case XamlNodeType.GetObject:
							renderer.WriteGetObject();
							break;
						case XamlNodeType.EndObject:
							renderer.WriteEndObject();
							break;

						case XamlNodeType.StartMember:
							renderer.WriteStartMember(xaml.Member);
							break;
						case XamlNodeType.EndMember:
							renderer.WriteEndMember();
							break;
						case XamlNodeType.Value:
							if (xaml.Value is string text)
								renderer.WriteValue(text);
							else
								renderer.WriteValue(xaml.Value.ToString()); // todo: use xaml to text converter
							break;
						default:
							throw new InvalidOperationException();
					}
				}
			}
		} // proc Write
	} // class EntityInlineRenderer

	#endregion

	#region -- class LineBreakInlineRenderer ------------------------------------------

	public class LineBreakInlineRenderer : XamlObjectRenderer<LineBreakInline>
	{
		protected override void Write(XamlMarkdownWriter renderer, LineBreakInline obj)
		{
			if (obj.IsHard)
				renderer.WriteLineBreak();
			else // Soft line break.
				renderer.WriteText(" ");
		} // proc Write
	} // class LineBreakInlineRenderer

	#endregion

	#region -- class LinkInlineRenderer -----------------------------------------------

	public class LinkInlineRenderer : XamlObjectRenderer<LinkInline>
	{
		protected override void Write(XamlMarkdownWriter renderer, LinkInline link)
		{
			var url = link.GetDynamicUrl != null ? link.GetDynamicUrl() ?? link.Url : link.Url;

			if (link.IsImage)
			{
				if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
					url = "#";

				renderer.WriteStartObject(typeof(Image));
				renderer.WriteResourceMember(null, MarkdownXamlStyle.Image);
				if (!String.IsNullOrEmpty(link.Title))
					renderer.WriteMember(ToolTipService.ToolTipProperty, link.Title);
				renderer.WriteMember(Image.SourceProperty, new Uri(url, UriKind.RelativeOrAbsolute));
				renderer.WriteEndObject();
			}
			else
			{
				WriteStartHyperlink(renderer, url, link.Title);
				renderer.WriteItems(link);
				WriteEndHyperlink(renderer);
			}
		} // proc Write

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteStartHyperlink(XamlMarkdownWriter renderer, string url, string linkTitle)
		{
			// check for valid url
			if (!url.StartsWith("#")
				&& !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
				url = "#";

			renderer.WriteStartObject(typeof(Hyperlink));
			renderer.WriteResourceMember(null, MarkdownXaml.HyperlinkStyleKey);
			//renderer.WriteMember(Hyperlink.CommandProperty, Commands.Hyperlink);
			//renderer.WriteMember(Hyperlink.CommandParameterProperty, url);
			renderer.WriteMember(Hyperlink.NavigateUriProperty, new Uri(url, UriKind.RelativeOrAbsolute));
			renderer.WriteMember(FrameworkContentElement.ToolTipProperty, String.IsNullOrEmpty(linkTitle) ? url : linkTitle);
		} // proc WriteStartHyperlink

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void WriteEndHyperlink(XamlMarkdownWriter renderer)
			=> renderer.WriteEndObject();
	} // class LinkInlineRenderer

	#endregion

	#region -- class LiteralInlineRenderer --------------------------------------------

	public class LiteralInlineRenderer : XamlObjectRenderer<LiteralInline>
	{
		protected override void Write(XamlMarkdownWriter renderer, LiteralInline obj)
		{
			if (obj.Content.IsEmpty)
				return;

			renderer.WriteText(ref obj.Content);
		} // proc Write
	} // class LiteralInlineRenderer

	#endregion
}
