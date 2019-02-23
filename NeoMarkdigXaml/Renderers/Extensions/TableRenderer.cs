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
using Markdig.Extensions.Tables;
using MdTable = Markdig.Extensions.Tables.Table;
using MdTableCell = Markdig.Extensions.Tables.TableCell;
using MdTableRow = Markdig.Extensions.Tables.TableRow;
using WpfTable = System.Windows.Documents.Table;
using WpfTableCell = System.Windows.Documents.TableCell;
using WpfTableColumn = System.Windows.Documents.TableColumn;
using WpfTableRow = System.Windows.Documents.TableRow;
using WpfTableRowGroup = System.Windows.Documents.TableRowGroup;

namespace Neo.Markdig.Xaml.Renderers.Extensions
{
	#region -- class TableRenderer ----------------------------------------------------

	public class TableRenderer : XamlObjectRenderer<MdTable>
    {
        protected override void Write(XamlMarkdownWriter renderer, MdTable table)
        {
            renderer.WriteStartObject(typeof(WpfTable));
            renderer.WriteResourceMember(null, MarkdownXamlStyle.Table);
            var t = new WpfTable();
            
            renderer.WriteStartItems(nameof(WpfTable.Columns));
            foreach(var col in table.ColumnDefinitions)
            {
                renderer.WriteStartObject(typeof(WpfTableColumn));
                renderer.WriteMember(nameof(WpfTableColumn.Width),
                    (col?.Width ?? 0) != 0
                        ? new GridLength(col.Width, GridUnitType.Star)
                        : GridLength.Auto
                );
                renderer.WriteEndObject();
            }
            renderer.WriteEndItems();

            renderer.WriteStartItems(nameof(WpfTable.RowGroups));
            renderer.WriteStartObject(typeof(WpfTableRowGroup));
            renderer.WriteStartItems(nameof(WpfTableRowGroup.Rows));

            foreach (var c in table)
            {
                var row = (MdTableRow)c;
                renderer.WriteStartObject(typeof(WpfTableRow));
                if (row.IsHeader)
                    renderer.WriteResourceMember(null, MarkdownXamlStyle.TableHeader);
                renderer.WriteStartItems(nameof(WpfTableRow.Cells));

                for (var i = 0; i < row.Count; i++)
                {
                    var cell = (MdTableCell)row[i];
                    renderer.WriteStartObject(typeof(WpfTableCell));
                    renderer.WriteResourceMember(null, MarkdownXamlStyle.TableCell);

                    if (cell.ColumnSpan > 1)
                        renderer.WriteMember(nameof(WpfTableCell.ColumnSpan), cell.ColumnSpan);
                    if (cell.RowSpan > 1)
                        renderer.WriteMember(nameof(WpfTableCell.RowSpan), cell.RowSpan);

                    var columnIndex = cell.ColumnIndex < 0 || cell.ColumnIndex >= table.ColumnDefinitions.Count ? i : cell.ColumnIndex;
                    columnIndex = columnIndex >= table.ColumnDefinitions.Count ? table.ColumnDefinitions.Count - 1 : columnIndex;
                    var alignment = table.ColumnDefinitions[columnIndex].Alignment;
                    if (alignment.HasValue)
                    {
                        switch (alignment)
                        {
                            case TableColumnAlign.Center:
                                renderer.WriteMember(nameof(WpfTableCell.TextAlignment), TextAlignment.Center);
                                break;
                            case TableColumnAlign.Right:
                                renderer.WriteMember(nameof(WpfTableCell.TextAlignment), TextAlignment.Right);
                                break;
                            case TableColumnAlign.Left:
                                renderer.WriteMember(nameof(WpfTableCell.TextAlignment), TextAlignment.Left);
                                break;
                        }
                    }

                    renderer.WriteItems(cell);

                    renderer.WriteEndObject();
                }

                renderer.WriteEndItems();
                renderer.WriteEndObject();
            }

            renderer.WriteEndItems();
            renderer.WriteEndObject();
            renderer.WriteEndItems();

            renderer.WriteEndObject();
        } // proc Write
	} // class TableRenderer

	#endregion
}
