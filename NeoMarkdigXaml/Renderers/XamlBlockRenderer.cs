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
using System.Windows;
using System.Windows.Documents;
using Markdig.Syntax;

namespace Neo.Markdig.Xaml.Renderers
{
	#region -- class ParagraphRenderer ------------------------------------------------

	public class ParagraphRenderer : XamlObjectRenderer<ParagraphBlock>
	{
		protected override void Write(XamlMarkdownWriter renderer, ParagraphBlock paragraph)
		{
			renderer.WriteStartObject(typeof(Paragraph));
			renderer.WriteItems(paragraph);
			renderer.WriteEndObject();
		} // proc Write
	} // class ParagraphRenderer

	#endregion

	#region -- class HeadingRenderer --------------------------------------------------

	public class HeadingRenderer : XamlObjectRenderer<HeadingBlock>
	{
		private static string GetStyleKey(int level)
		{
			switch (level)
			{
				case 1:
					return "xmarkdig:MarkdownXaml.Heading1StyleKey";
				case 2:
					return "xmarkdig:MarkdownXaml.Heading2StyleKey";
				case 3:
					return "xmarkdig:MarkdownXaml.Heading3StyleKey";
				case 4:
					return "xmarkdig:MarkdownXaml.Heading4StyleKey";
				case 5:
					return "xmarkdig:MarkdownXaml.Heading5StyleKey";
				default:
					return "xmarkdig:MarkdownXaml.Heading6StyleKey";
			}
		} // func GetStyleKey

		protected override void Write(XamlMarkdownWriter renderer, HeadingBlock headingBlock)
		{
			renderer.WriteStartObject(typeof(Paragraph));
			renderer.WriteStaticResourceMember(null, GetStyleKey(headingBlock.Level));
			renderer.WriteItems(headingBlock);
			renderer.WriteEndObject();
		} // proc Write
	} // class HeadingRenderer

	#endregion

	#region -- class CodeBlockRenderer ------------------------------------------------

	public class CodeBlockRenderer : XamlObjectRenderer<CodeBlock>
	{
		protected override void Write(XamlMarkdownWriter renderer, CodeBlock obj)
		{
			renderer.WriteStartObject(typeof(Paragraph));
			renderer.WriteStaticResourceMember(null, "xmarkdig:MarkdownXaml.CodeBlockStyleKey");
			//if (obj is FencedCodeBlock f)
			//    f.Info;
			renderer.WriteItems(obj, true);
			renderer.WriteEndObject();
		} // proc Write
	} // class CodeBlockRenderer

	#endregion

	#region -- class ListRenderer -----------------------------------------------------

	public class ListRenderer : XamlObjectRenderer<ListBlock>
	{
		protected override void Write(XamlMarkdownWriter renderer, ListBlock listBlock)
		{
			renderer.WriteStartObject(typeof(List));

			if (listBlock.IsOrdered)
			{
				renderer.WriteMember(List.MarkerStyleProperty, TextMarkerStyle.Decimal);

				if (listBlock.OrderedStart != null && (listBlock.DefaultOrderedStart != listBlock.OrderedStart))
					renderer.WriteMember(List.StartIndexProperty, listBlock.OrderedStart);
			}
			else
				renderer.WriteMember(List.MarkerStyleProperty, TextMarkerStyle.Disc);

			renderer.WriteStartItems(nameof(List.ListItems));

			foreach (var cur in listBlock)
			{
				renderer.WriteStartObject(typeof(ListItem));
				renderer.WriteItems((ContainerBlock)cur);
				renderer.WriteEndObject();
			}

			renderer.WriteEndItems();
			renderer.WriteEndObject();
		} // proc Write
	} // class ListRenderer

	#endregion

	#region -- class QuoteBlockRenderer -----------------------------------------------

	public class QuoteBlockRenderer : XamlObjectRenderer<QuoteBlock>
	{
		protected override void Write(XamlMarkdownWriter renderer, QuoteBlock block)
		{
			renderer.WriteStartObject(typeof(Section));
			renderer.WriteStaticResourceMember(null, "xmarkdig:MarkdownXaml.QuoteBlockStyleKey");

			renderer.WriteItems(block);

			renderer.WriteEndObject();
		} // proc Write
	} // class QuoteBlockRenderer

	#endregion

	#region -- class ThematicBreakRenderer --------------------------------------------

	public class ThematicBreakRenderer : XamlObjectRenderer<ThematicBreakBlock>
	{
		protected override void Write(XamlMarkdownWriter renderer, ThematicBreakBlock obj)
		{
			renderer.WriteStartObject(typeof(Paragraph));
			renderer.WriteStaticResourceMember(null, "xmarkdig:MarkdownXaml.ThematicBreakStyleKey");
			renderer.WriteEndObject();

			//var line = new System.Windows.Shapes.Line { X2 = 1 };
			//line.SetResourceReference(FrameworkContentElement.StyleProperty, Styles.ThematicBreakStyleKey);

			//var paragraph = new Paragraph
			//{
			//    Inlines = { new InlineUIContainer(line) }
			//};

			//renderer.WriteBlock(paragraph);
		} // proc Write
	} // class ThematicBreakRenderer

	#endregion
}
