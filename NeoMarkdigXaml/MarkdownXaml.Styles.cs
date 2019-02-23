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
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Neo.Markdig.Xaml
{
	/// <summary></summary>
	public enum MarkdownXamlStyle
	{
		/// <summary>Resource Key for the DocumentStyle.</summary>
		Document,
		/// <summary>Resource Key for the CodeStyle.</summary>
		Code,
		/// <summary>Resource Key for the CodeBlockStyle.</summary>
		CodeBlock,
		/// <summary>Resource Key for the Heading1Style.</summary>
		Heading1,
		/// <summary>Resource Key for the Heading2Style.</summary>
		Heading2,
		/// <summary>Resource Key for the Heading3Style.</summary>
		Heading3,
		/// <summary>Resource Key for the Heading4Style.</summary>
		Heading4,
		/// <summary>Resource Key for the Heading5Style.</summary>
		Heading5,
		/// <summary>Resource Key for the Heading6Style.</summary>
		Heading6,
		/// <summary>Resource Key for the ImageStyle.</summary>
		Image,
		/// <summary>Resource Key for the InsertedStyle.</summary>
		Inserted,
		/// <summary>Resource Key for the MarkedStyle.</summary>
		Marked,
		/// <summary>Resource Key for the QuoteBlockStyle.</summary>
		QuoteBlock,
		/// <summary>Resource Key for the StrikeThroughStyle.</summary>
		StrikeThrough,  
		/// <summary>Resource Key for the SubscriptStyle.</summary>
		Subscript,
		/// <summary>Resource Key for the SuperscriptStyle.</summary>
		Superscript,
		/// <summary>Resource Key for the TableStyle.</summary>
		Table,
		/// <summary>Resource Key for the TableCellStyle.</summary>
		TableCell,
		/// <summary>Resource Key for the TableHeaderStyle.</summary>
		TableHeader,
		/// <summary>Resource Key for the TaskListStyle.</summary>
		TaskList,
		/// <summary>Resource Key for the ThematicBreakStyle.</summary>
		ThematicBreak,
		/// <summary></summary>
		Hyperlink
	} // enum MarkdownXamlStyle

	public static partial class MarkdownXaml
	{
		#region -- class StyleResourceKey----------------------------------------------

		private sealed class StyleResourceKey : ResourceKey
		{
			public StyleResourceKey(string key)
				=> Key = key;

			public override string ToString()
				=> "XamlStyle: " + Key;

			public override bool Equals(object obj)
				=> obj is StyleResourceKey srk ? Key.Equals(srk.Key) : base.Equals(obj);

			public override int GetHashCode()
				=> Assembly.GetHashCode() ^ Key.GetHashCode();

			public string Key { get; }
			public override Assembly Assembly => typeof(StyleResourceKey).Assembly;
		} // class StyleResourceKey

		#endregion

		/// <summary>Resource Key for the DocumentStyle.</summary>
		public static ResourceKey DocumentStyleKey { get; } = new StyleResourceKey(nameof(DocumentStyleKey));

		/// <summary>Resource Key for the CodeStyle.</summary>
		public static ResourceKey CodeStyleKey { get; } = new StyleResourceKey(nameof(CodeStyleKey));

		/// <summary>Resource Key for the CodeBlockStyle.</summary>
		public static ResourceKey CodeBlockStyleKey { get; } = new StyleResourceKey(nameof(CodeBlockStyleKey));

		/// <summary>Resource Key for the Heading1Style.</summary>
		public static ResourceKey Heading1StyleKey { get; } = new StyleResourceKey(nameof(Heading1StyleKey));

		/// <summary>Resource Key for the Heading2Style.</summary>
		public static ResourceKey Heading2StyleKey { get; } = new StyleResourceKey(nameof(Heading2StyleKey));

		/// <summary>Resource Key for the Heading3Style.</summary>
		public static ResourceKey Heading3StyleKey { get; } = new StyleResourceKey(nameof(Heading3StyleKey));

		/// <summary>Resource Key for the Heading4Style.</summary>
		public static ResourceKey Heading4StyleKey { get; } = new StyleResourceKey(nameof(Heading4StyleKey));

		/// <summary>Resource Key for the Heading5Style.</summary>
		public static ResourceKey Heading5StyleKey { get; } = new StyleResourceKey(nameof(Heading5StyleKey));

		/// <summary>Resource Key for the Heading6Style.</summary>
		public static ResourceKey Heading6StyleKey { get; } = new StyleResourceKey(nameof(Heading6StyleKey));

		/// <summary>Resource Key for the ImageStyle.</summary>
		public static ResourceKey ImageStyleKey { get; } = new StyleResourceKey(nameof(ImageStyleKey));

		/// <summary>Resource Key for the InsertedStyle.</summary>
		public static ResourceKey InsertedStyleKey { get; } = new StyleResourceKey(nameof(InsertedStyleKey));

		/// <summary>Resource Key for the MarkedStyle.</summary>
		public static ResourceKey MarkedStyleKey { get; } = new StyleResourceKey(nameof(MarkedStyleKey));

		/// <summary>Resource Key for the QuoteBlockStyle.</summary>
		public static ResourceKey QuoteBlockStyleKey { get; } = new StyleResourceKey(nameof(QuoteBlockStyleKey));

		/// <summary>Resource Key for the StrikeThroughStyle.</summary>
		public static ResourceKey StrikeThroughStyleKey { get; } = new StyleResourceKey(nameof(StrikeThroughStyleKey));
		/// <summary>Resource Key for the SubscriptStyle.</summary>
		public static ResourceKey SubscriptStyleKey { get; } = new StyleResourceKey(nameof(SubscriptStyleKey));

		/// <summary>Resource Key for the SuperscriptStyle.</summary>
		public static ResourceKey SuperscriptStyleKey { get; } = new StyleResourceKey(nameof(SuperscriptStyleKey));

		/// <summary>Resource Key for the TableStyle.</summary>
		public static ResourceKey TableStyleKey { get; } = new StyleResourceKey(nameof(TableStyleKey));

		/// <summary>Resource Key for the TableCellStyle.</summary>
		public static ResourceKey TableCellStyleKey { get; } = new StyleResourceKey(nameof(TableCellStyleKey));

		/// <summary>Resource Key for the TableHeaderStyle.</summary>
		public static ResourceKey TableHeaderStyleKey { get; } = new StyleResourceKey(nameof(TableHeaderStyleKey));

		/// <summary>Resource Key for the TaskListStyle.</summary>
		public static ResourceKey TaskListStyleKey { get; } = new StyleResourceKey(nameof(TaskListStyleKey));

		/// <summary>Resource Key for the ThematicBreakStyle.</summary>
		public static ResourceKey ThematicBreakStyleKey { get; } = new StyleResourceKey(nameof(ThematicBreakStyleKey));

		/// <summary></summary>
		public static ResourceKey HyperlinkStyleKey { get; } = new StyleResourceKey(nameof(HyperlinkStyleKey));

		/// <summary>Routed command for Hyperlink.</summary>
		public static RoutedCommand Hyperlink { get; } = new RoutedCommand(nameof(Hyperlink), typeof(MarkdownXaml));
	} // classs MarkdownXaml
}
