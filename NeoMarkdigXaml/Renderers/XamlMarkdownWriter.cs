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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Xaml;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Neo.Markdig.Xaml.Renderers.Extensions;
using Neo.Markdig.Xaml.Renderers.Inlines;

namespace Neo.Markdig.Xaml.Renderers
{
	#region -- class XamlMarkdownWriter -----------------------------------------------

	/// <summary>XAML token stream generator for a Markdown <see cref="MarkdownDocument"/> object.</summary>
	public class XamlMarkdownWriter : RendererBase, System.Windows.Markup.IUriContext
	{
		#region -- class StaticResourceKeyStoreInfo -----------------------------------

		private sealed class StaticResourceKeyStoreInfo
		{
			public StaticResourceKeyStoreInfo(Type type, string prefix)
			{
				Type = type ?? throw new ArgumentNullException(nameof(type));
				Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
			} // ctor

			public Type Type { get; }
			public string Prefix { get; }
		} // class ResourceKeyStoreInfo

		#endregion

		#region -- struct StaticResourceKeyInfo ---------------------------------------

		private struct StaticResourceKeyInfo
		{
			public StaticResourceKeyInfo(StaticResourceKeyStoreInfo keyStoreInfo, string memberName)
			{
				KeyStore = keyStoreInfo ?? throw new ArgumentNullException(nameof(keyStoreInfo));
				MemberName = memberName ?? throw new ArgumentNullException(nameof(memberName));
			} // ctor

			public string MemberName { get; }
			public StaticResourceKeyStoreInfo KeyStore { get; }
		} // struct StaticResourceKeyInfo

		#endregion

		private readonly XamlWriter writer;
		private readonly Stack<XamlType> xamlTypes = new Stack<XamlType>();

		private readonly Dictionary<ResourceKey, StaticResourceKeyInfo> staticResourceKeys = new Dictionary<ResourceKey, StaticResourceKeyInfo>();

		private readonly XamlType runType;
		private readonly XamlMember runTextMember;

		private XamlMember currentContentMember = null; // start a _Items access before the next object

		private bool preserveWhitespace = false; // preserve current whitespaces
		private bool appendWhiteSpace = false;
		private bool firstCharOfBlock = true;
		private readonly StringBuilder textBuffer = new StringBuilder(); // current text buffer to collect all words

		#region -- Ctor/Dtor ----------------------------------------------------------

		/// <summary>Initializes a new instance of the <see cref="XamlMarkdownWriter"/> class.</summary>
		/// <param name="writer">Target for the xaml content.</param>
		public XamlMarkdownWriter(XamlWriter writer)
		{
			this.writer = writer ?? throw new ArgumentNullException(nameof(writer));

			this.runType = SchemaContext.GetXamlType(typeof(Run)) ?? throw new ArgumentNullException(nameof(Run));
			this.runTextMember = runType.GetMember(nameof(Run.Text)) ?? throw new ArgumentNullException(nameof(Run.Text));

			// Default block renderers
			ObjectRenderers.Add(new CodeBlockRenderer());
			ObjectRenderers.Add(new ListRenderer());
			ObjectRenderers.Add(new HeadingRenderer());
			ObjectRenderers.Add(new ParagraphRenderer());
			ObjectRenderers.Add(new QuoteBlockRenderer());
			ObjectRenderers.Add(new ThematicBreakRenderer());

			// Default inline renderers
			ObjectRenderers.Add(new AutolinkInlineRenderer());
			ObjectRenderers.Add(new CodeInlineRenderer());
			ObjectRenderers.Add(new DelimiterInlineRenderer());
			ObjectRenderers.Add(new EmphasisInlineRenderer());
			ObjectRenderers.Add(new EntityInlineRenderer());
			ObjectRenderers.Add(new LineBreakInlineRenderer());
			ObjectRenderers.Add(new LinkInlineRenderer());
			ObjectRenderers.Add(new LiteralInlineRenderer());

			// Extension renderers
			ObjectRenderers.Add(new TableRenderer());
			ObjectRenderers.Add(new TaskListRenderer());
		} // ctor

		#endregion

		#region -- Primitives ---------------------------------------------------------

		/// <summary></summary>
		/// <param name="ns"></param>
		/// <param name="prefix"></param>
		public void WriteNamespace(string ns, string prefix)
			=> WriteNamespace(new NamespaceDeclaration(ns, prefix));

		/// <summary></summary>
		public void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
			=> writer.WriteNamespace(namespaceDeclaration);

		/// <summary>Start to write a xaml-object.</summary>
		/// <param name="type"></param>
		public XamlType WriteStartObject(Type type)
		{
			var xamlType = SchemaContext.GetXamlType(type ?? throw new ArgumentNullException(nameof(type)))
				?? throw new ArgumentOutOfRangeException(nameof(type), type, "Could not resolve xaml type.");

			return WriteStartObject(xamlType);
		} // proc WriteStartObject

		/// <summary>Start to write a xaml-object.</summary>
		/// <param name="xamlType"></param>
		/// <returns></returns>
		public XamlType WriteStartObject(XamlType xamlType)
		{
			xamlTypes.Push(xamlType);

			// write pending elements
			WritePendingStartItems();
			WritePendingText(true);

			writer.WriteStartObject(xamlType);

			return xamlType;
		} // proc WriteStartObject

		/// <summary>Write GetObject</summary>
		public void WriteGetObject()
			=> writer.WriteGetObject();

		/// <summary>Closes the current object.</summary>
		/// <returns>Current evaluated object or null.</returns>
		public object WriteEndObject()
		{
			// write pending text
			WritePendingText(false);

			xamlTypes.Pop();
			writer.WriteEndObject();
			return GetResult();
		} // proc WriteEndObject

		private object GetResult()
			=> writer is XamlObjectWriter ow
				? ow.Result
				: null;

		/// <summary>Get a xaml member to a member string.</summary>
		/// <param name="memberName"></param>
		/// <returns></returns>
		public XamlMember GetMember(string memberName)
		{
			var xamlType = xamlTypes.Peek();
			return xamlType.GetMember(memberName ?? throw new ArgumentNullException(nameof(memberName)));
		}

		/// <summary>Get a xaml member to a dependency property.</summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public XamlMember GetMember(DependencyProperty property)
		{
			if (property == null)
				throw new ArgumentNullException(nameof(property));

			var xamlType = xamlTypes.Peek();
			if (property.OwnerType.IsAssignableFrom(xamlType.UnderlyingType))
				return xamlType.GetMember(property.Name);
			else
			{
				var type = SchemaContext.GetXamlType(property.OwnerType);
				return type.GetAttachableMember(property.Name);
			}
		}

		/// <summary>Start a member.</summary>
		/// <param name="memberName"></param>
		public void WriteStartMember(string memberName)
			=> WriteStartMember(GetMember(memberName));

		/// <summary>Start a member.</summary>
		/// <param name="property"></param>
		public void WriteStartMember(DependencyProperty property)
			=> WriteStartMember(GetMember(property));

		/// <summary>Start a member.</summary>
		/// <param name="member"></param>
		public void WriteStartMember(XamlMember member)
			=> writer.WriteStartMember(member ?? throw new ArgumentNullException(nameof(member)));

		/// <summary>End the current member.</summary>
		public void WriteEndMember()
		{
			WritePendingText(false);
			writer.WriteEndMember();
		} // proc WriteEndMember

		/// <summary>Start a items collection.</summary>
		/// <param name="memberName"></param>
		/// <param name="preserveSpaces"></param>
		public void WriteStartItems(string memberName, bool preserveSpaces = false)
			=> WriteStartItems(GetMember(memberName), preserveSpaces);

		/// <summary>Start a items collection.</summary>
		/// <param name="property"></param>
		/// <param name="preserveSpaces"></param>
		public void WriteStartItems(DependencyProperty property, bool preserveSpaces = false)
			=> WriteStartItems(GetMember(property), preserveSpaces);

		/// <summary>Start a items collection.</summary>
		/// <param name="member"></param>
		/// <param name="preserveSpaces"></param>
		public void WriteStartItems(XamlMember member, bool preserveSpaces = false)
		{
			if (currentContentMember != null)
				throw new InvalidOperationException();

			preserveWhitespace = preserveSpaces;
			appendWhiteSpace = false;
			firstCharOfBlock = true;
			currentContentMember = member;
		} // proc WriteStartItems

		private void WritePendingStartItems()
		{
			if (currentContentMember != null)
			{
				WriteStartMember(currentContentMember);
				writer.WriteGetObject();
				writer.WriteStartMember(XamlLanguage.Items);

				currentContentMember = null;
			}
		} // proc WritePendingStartItems

		/// <summary>End a items collection.</summary>
		public void WriteEndItems()
		{
			WritePendingText(false);

			if (currentContentMember == null)
			{
				writer.WriteEndMember();
				writer.WriteEndObject();
				writer.WriteEndMember();
			}
			else
				currentContentMember = null;
		}

		/// <summary>Write a complete member.</summary>
		/// <param name="memberName"></param>
		/// <param name="value"></param>
		public void WriteMember(string memberName, object value)
		{
			if (value != null)
				WriteMember(GetMember(memberName), value);
		} // proc WriteMember

		/// <summary>Write a complete member.</summary>
		/// <param name="property"></param>
		/// <param name="value"></param>
		public void WriteMember(DependencyProperty property, object value)
		{
			if (value != null)
				WriteMember(GetMember(property), value);
		} // proc WriteMember

		/// <summary>Write a complete member.</summary>
		/// <param name="member"></param>
		/// <param name="value"></param>
		public void WriteMember(XamlMember member, object value)
		{
			if (value == null)
				return;
			if (IsPendingText)
				throw new InvalidOperationException("Start member during text collection.");

			writer.WriteStartMember(member);
			if (writer is XamlObjectWriter)
				writer.WriteValue(value);
			else
			{
				if (!(value is string str))
					str = member.TypeConverter.ConverterInstance.ConvertToString(value);

				if (str != null)
					writer.WriteValue(str);
			}
			writer.WriteEndMember();
		} // proc WriteMember

		/// <summary>Write Inline LineBreak</summary>
		public void WriteLineBreak()
		{
			WriteStartObject(typeof(LineBreak));
			WriteEndObject();
		} // proc WriteLineBreak

		/// <summary>Write value</summary>
		/// <param name="value"></param>
		public void WriteValue(string value)
			=> WriteText(value);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AppendChar(char c)
		{
			if (Char.IsWhiteSpace(c))
				appendWhiteSpace = true;
			else
			{
				if (appendWhiteSpace)
				{
					if (!firstCharOfBlock)
						textBuffer.Append(' ');
					appendWhiteSpace = false;
				}

				firstCharOfBlock = false;
				textBuffer.Append(c);
			}
		}

		/// <summary>Write normal text.</summary>
		/// <param name="slice"></param>
		public void WriteText(ref StringSlice slice)
		{
			if (slice.Start > slice.End)
				return;

			if (preserveWhitespace)
				textBuffer.Append(slice.Text, slice.Start, slice.Length);
			else
			{
				for (var i = slice.Start; i <= slice.End; i++)
				{
					var c = slice[i];
					AppendChar(c);
				}
			}
		} // proc WriteText

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal void WriteText(string text)
		{
			if (preserveWhitespace)
				textBuffer.Append(text);
			else
			{
				var l = text.Length;
				for (var i = 0; i < l; i++)
					AppendChar(text[i]);
			}
		} // proc WriteText

		private void WritePendingText(bool onStartObject)
		{
			if (IsPendingText)
			{
				WritePendingStartItems();

				if (preserveWhitespace)
				{
					writer.WriteStartObject(runType);
					writer.WriteStartMember(runTextMember);
					writer.WriteValue(textBuffer.ToString());
					writer.WriteEndMember();
					writer.WriteEndObject();
					textBuffer.Length = 0;
				}
				else
				{
					if (appendWhiteSpace && onStartObject)
					{
						textBuffer.Append(' ');
						appendWhiteSpace = false;
					}

					writer.WriteValue(textBuffer.ToString());
					textBuffer.Length = 0;
				}
			}
		} // proc WritePendingText

		private XamlMember GetContentProperty()
			=> xamlTypes.Peek().ContentProperty ?? throw new ArgumentNullException(nameof(XamlType.ContentProperty));

		/// <summary>Write inline stream in the current type.</summary>
		/// <param name="leafBlock"></param>
		/// <param name="preserveSpaces"></param>
		public void WriteItems(LeafBlock leafBlock, bool preserveSpaces = false)
		{
			if (leafBlock == null)
				throw new ArgumentNullException(nameof(leafBlock));

			WriteStartItems(GetContentProperty(), preserveSpaces);

			if (leafBlock.Inline != null)
			{
				WriteChildren(leafBlock.Inline);
			}
			else
			{
				var lineCount = leafBlock.Lines.Count;
				var first = true;
				for (var i = 0; i < lineCount; i++)
				{
					if (first)
						first = false;
					else if (preserveSpaces)
						WriteLineBreak();
					else
						AppendChar(' ');

					WriteText(ref leafBlock.Lines.Lines[i].Slice);
				}
			}

			WriteEndItems();
		} // proc WriteItems

		/// <summary>Write inline stream in the current type.</summary>
		/// <param name="inlines"></param>
		/// <param name="preserveSpaces"></param>
		public void WriteItems(ContainerInline inlines, bool preserveSpaces = false)
		{
			if (inlines == null)
				throw new ArgumentNullException(nameof(inlines));

			WriteStartItems(GetContentProperty(), preserveSpaces);
			WriteChildren(inlines);
			WriteEndItems();
		} // proc WriteItems

		/// <summary>Write block stream in the current type.</summary>
		/// <param name="block"></param>
		/// <param name="preserveSpaces"></param>
		public void WriteItems(ContainerBlock block, bool preserveSpaces = false)
		{
			if (block == null)
				throw new ArgumentNullException(nameof(block));

			WriteStartItems(GetContentProperty(), preserveSpaces);
			WriteChildren(block);
			WriteEndItems();
		} // proc WriteItems

		/// <summary>Write text as content in the current type.</summary>
		/// <param name="text"></param>
		/// <param name="preserveSpaces"></param>
		public void WriteItems(string text, bool preserveSpaces = false)
		{
			WriteStartItems(GetContentProperty(), preserveSpaces);
			WriteText(text);
			WriteEndItems();
		} // proc WriteItems

		private bool IsPendingText => textBuffer.Length > 0;

		#endregion

		#region -- Resources ----------------------------------------------------------

		protected void RegisterResourceKeys(Type resourceKeyStoreType, string prefix)
		{
			var resourceKeyStore = new StaticResourceKeyStoreInfo(resourceKeyStoreType, prefix);
			var itemsAdded = false;

			// register resources
			foreach (var pi in (from p in resourceKeyStoreType.GetRuntimeProperties() where p.CanRead && p.GetMethod.IsPublic && p.GetMethod.IsStatic && typeof(ResourceKey).IsAssignableFrom(p.PropertyType) select p))
			{
				var rk = (ResourceKey)pi.GetValue(null);
				staticResourceKeys.Add(rk, new StaticResourceKeyInfo(resourceKeyStore, pi.Name));
				itemsAdded = true;
			}

			// add name space
			if (itemsAdded)
				WriteNamespace("clr-namespace:" + resourceKeyStoreType.Assembly.GetName().Name + ";assembly=" + resourceKeyStoreType.Namespace, prefix);
		} // proc RegisterResourceKeys

		public void WriteResourceMember(XamlMember member, object value)
		{
			WriteStartMember(member ?? GetMember("Style"));
			WriteStartObject(typeof(StaticResourceExtension));
			WriteStartMember(XamlLanguage.PositionalParameters);

			WriteMemberValueCore(value);

			WriteEndMember();
			WriteEndObject();
			WriteEndMember();
		} // proc WriteResourceMember

		protected virtual void WriteMemberValueCore(object value)
		{
			if (value is MarkdownXamlStyle style)
				value = GetDefaultStyle(style);

			if (writer is XamlObjectWriter)
				writer.WriteValue(value);
			else if (!TryWriteResouceKey(value as ResourceKey))
				writer.WriteValue(value);
		} // proc WriteMemberValueCore

		public bool TryWriteResouceKey(ResourceKey resourceKey)
		{
			if (resourceKey != null && staticResourceKeys.TryGetValue(resourceKey, out var info)) // known resource key
			{
				WriteStartObject(XamlLanguage.Static);
				WriteStartMember(XamlLanguage.PositionalParameters);

				writer.WriteValue(info.KeyStore.Prefix + ":" + info.KeyStore.Type.Name + "." + info.MemberName);

				WriteEndMember();
				WriteEndObject();

				return true;
			}
			else
				return false;
		} // proc WriteResouceKey

		public virtual object GetDefaultStyle(MarkdownXamlStyle style)
		{
			switch (style)
			{
				case MarkdownXamlStyle.Document:
					return MarkdownXaml.DocumentStyleKey;
				case MarkdownXamlStyle.Code:
					return MarkdownXaml.CodeStyleKey;
				case MarkdownXamlStyle.CodeBlock:
					return MarkdownXaml.CodeBlockStyleKey;
				case MarkdownXamlStyle.Heading1:
					return MarkdownXaml.Heading1StyleKey;
				case MarkdownXamlStyle.Heading2:
					return MarkdownXaml.Heading2StyleKey;
				case MarkdownXamlStyle.Heading3:
					return MarkdownXaml.Heading3StyleKey;
				case MarkdownXamlStyle.Heading4:
					return MarkdownXaml.Heading4StyleKey;
				case MarkdownXamlStyle.Heading5:
					return MarkdownXaml.Heading5StyleKey;
				case MarkdownXamlStyle.Heading6:
					return MarkdownXaml.Heading6StyleKey;
				case MarkdownXamlStyle.Image:
					return MarkdownXaml.ImageStyleKey;
				case MarkdownXamlStyle.Inserted:
					return MarkdownXaml.InsertedStyleKey;
				case MarkdownXamlStyle.Marked:
					return MarkdownXaml.MarkedStyleKey;
				case MarkdownXamlStyle.QuoteBlock:
					return MarkdownXaml.QuoteBlockStyleKey;
				case MarkdownXamlStyle.StrikeThrough:
					return MarkdownXaml.StrikeThroughStyleKey;
				case MarkdownXamlStyle.Subscript:
					return MarkdownXaml.SubscriptStyleKey;
				case MarkdownXamlStyle.Superscript:
					return MarkdownXaml.SuperscriptStyleKey;
				case MarkdownXamlStyle.Table:
					return MarkdownXaml.TableStyleKey;
				case MarkdownXamlStyle.TableCell:
					return MarkdownXaml.TableCellStyleKey;
				case MarkdownXamlStyle.TableHeader:
					return MarkdownXaml.TableHeaderStyleKey;
				case MarkdownXamlStyle.TaskList:
					return MarkdownXaml.TaskListStyleKey;
				case MarkdownXamlStyle.ThematicBreak:
					return MarkdownXaml.ThematicBreakStyleKey;
				case MarkdownXamlStyle.Hyperlink:
					return MarkdownXaml.HyperlinkStyleKey;
				default:
					throw new ArgumentOutOfRangeException(nameof(style));
			}
		} // func GetDefaultStyle

		#endregion

		#region -- Render -------------------------------------------------------------

		protected virtual void WriteNamespaces()
			=> RegisterResourceKeys(typeof(MarkdownXaml), "xmarkdig");

		protected virtual void WriteResources()
		{
			//WriteStartItems(nameof(FlowDocument.Resources));
			//WriteEndItems();
		} // proc WriteResources

		/// <summary>Render the markdown object in a XamlWriter.</summary>
		/// <param name="markdownObject"></param>
		/// <returns></returns>
		public override object Render(MarkdownObject markdownObject)
		{
			if (markdownObject is MarkdownDocument)
			{
				// emit namespaces
				WriteNamespace("http://schemas.microsoft.com/winfx/2006/xaml", "x");
				WriteNamespaces();

				// start flow document
				WriteStartObject(typeof(FlowDocument));
				WriteResources();

				WriteResourceMember(null, MarkdownXamlStyle.Document);

				WriteStartItems(nameof(FlowDocument.Blocks));

				Write(markdownObject);

				WriteEndItems();
				return WriteEndObject();
			}
			else
			{
				Write(markdownObject);
				return GetResult();
			}
		} // proc Render

		#endregion

		private object GetAbsoluteUri(Uri uri, bool enforceUri = true)
		{
			if (!enforceUri && uri.IsAbsoluteUri)
			{
				if (uri.Scheme == "file")
					return uri.AbsolutePath;
				else
					return uri;
			}
			else
				return uri;
		} // func GetAbsoluteUri

		public object GetUri(Uri uri, bool enforceUri = true)
		{
			if (uri == null)
				return null;
			else if (uri.IsAbsoluteUri)
				return GetAbsoluteUri(uri, enforceUri);
			else if (BaseUri != null)
				return GetAbsoluteUri(new Uri(BaseUri, uri), enforceUri);
			else
				return uri;
		} // func GetUri

		/// <summary>Define a BaseUri for the uri</summary>
		public Uri BaseUri { get; set; } = null;
		/// <summary>Acces to current schema context.</summary>
		public XamlSchemaContext SchemaContext => writer.SchemaContext;
	} // class XamlMarkdownWriter

	#endregion

	#region -- class XamlObjectRenderer -----------------------------------------------

	/// <summary>A base class for XAML rendering <see cref="Block"/> and <see cref="Syntax.Inlines.Inline"/> Markdown objects.</summary>
	/// <typeparam name="TObject">The type of the object.</typeparam>
	public abstract class XamlObjectRenderer<TObject> : MarkdownObjectRenderer<XamlMarkdownWriter, TObject>
		where TObject : MarkdownObject
	{
	} // class XamlObjectRenderer

	#endregion

	#region -- class Helper -----------------------------------------------------------

	internal static class Helper
	{
		public static bool TryGetProperty(this HtmlAttributes attributes, string name, out string value)
		{
			if (attributes != null)
			{
				var properties = attributes.Properties;
				for (var i = 0; i < properties.Count; i++)
				{
					if (properties[i].Key == name)
					{
						value = properties[i].Value;
						return true;
					}
				}
			}
			value = null;
			return false;
		} // func TryGetProperty

		public static bool TryGetPropertyInt32(this HtmlAttributes attributes, string name, out int value)
		{
			if (TryGetProperty(attributes, name, out var tmp) && Int32.TryParse(tmp, out value))
				return true;
			else
			{
				value = 0;
				return false;
			}
		} // func TryGetPropertyInt32
	} // class Helper

	#endregion
}
