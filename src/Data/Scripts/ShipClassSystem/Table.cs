using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace RedVsBlueClassSystem
{
    class Table
    {
        public List<Column> Columns = new List<Column>();

        public List<Row> Rows = new List<Row>();

        public void Clear()
        {
            Rows.Clear();
        }

        public void RenderToSprites(List<MySprite> sprites, Vector2 topLeft, float width, Vector2 cellGap, float scale = 1)
        {
            Vector2 ignored;

            RenderToSprites(sprites, topLeft, width, cellGap, out ignored, scale);
        }

        public void RenderToSprites(List<MySprite> sprites, Vector2 topLeft, float width, Vector2 cellGap, out Vector2 positionAfter, float scale = 1)
        {
            //Calculate column widths & row heights
            float[] columnContentWidths = new float[Columns.Count];
            float[] columnWidths = new float[Columns.Count];
            float[] rowHeights = new float[Rows.Count];
            float totalFreeSpaceWeight = 0;
            float minWidthRequired = 0;

            for (int colNum = 0; colNum < Columns.Count; colNum++)
            {
                totalFreeSpaceWeight += Columns[colNum].FreeSpace;

                for (int rowNum = 0; rowNum < Rows.Count; rowNum++)
                {
                    var row = Rows[rowNum];
                    var cell = row[colNum];

                    if (!cell.IsEmpty)
                    {
                        columnContentWidths[colNum] = Math.Max(columnContentWidths[colNum], TextUtils.GetTextWidth(cell.Value, scale));
                        rowHeights[rowNum] = Math.Max(rowHeights[rowNum], TextUtils.GetTextHeight(cell.Value, scale));
                    }
                }

                minWidthRequired += columnContentWidths[colNum] + (colNum > 0 ? cellGap.X : 0);
                columnWidths[colNum] = columnContentWidths[colNum];
            }

            //distribute free space
            if (minWidthRequired < width && totalFreeSpaceWeight > 0)
            {
                float freeSpace = width - minWidthRequired;

                for (int i = 0; i < Columns.Count; i++)
                {
                    if (Columns[i].FreeSpace > 0)
                    {
                        columnWidths[i] += freeSpace * (Columns[i].FreeSpace / totalFreeSpaceWeight);
                    }
                }
            }

            var rowTopLeft = topLeft;
            var tableHeight = 0f;

            //render rows
            for (int rowNum = 0; rowNum < Rows.Count; rowNum++)
            {
                var row = Rows[rowNum];

                float rowX = 0;

                for (int colNum = 0; colNum < Columns.Count; colNum++)
                {
                    var cell = row[colNum];
                    var column = Columns[colNum];

                    if (!cell.IsEmpty)
                    {
                        var sprite = MySprite.CreateText(cell.Value, "Monospace", cell.Color, scale, column.Alignment);

                        switch (column.Alignment)
                        {
                            case TextAlignment.LEFT:
                                sprite.Position = rowTopLeft + new Vector2(rowX, 0);
                                break;
                            case TextAlignment.RIGHT:
                                sprite.Position = rowTopLeft + new Vector2(rowX + columnWidths[colNum], 0);
                                break;
                            case TextAlignment.CENTER:
                                sprite.Position = rowTopLeft + new Vector2(rowX + (columnWidths[colNum] / 2), 0);

                                break;
                        }

                        sprites.Add(sprite);
                    }

                    rowX += columnWidths[colNum] + cellGap.X;
                }

                float rowTotalHeight = rowHeights[rowNum] + (rowNum > 0 ? cellGap.Y : 0);
                rowTopLeft += new Vector2(0, rowTotalHeight);
                tableHeight += rowTotalHeight;
            }

            positionAfter = topLeft + new Vector2(0, tableHeight);
        }

    }

    class Column
    {
        public string Name;
        public float FreeSpace = 0;
        public TextAlignment Alignment = TextAlignment.LEFT;
    }

    class Row : List<Cell>
    {

    }

    struct Cell
    {
        public string Value;
        public Color Color;

        public bool IsEmpty { get { return string.IsNullOrEmpty(Value); } }

        public Cell(string value, Color color)
        {
            Value = value;
            Color = color;
        }

        public Cell(string value)
        {
            Value = value;
            Color = Color.White;
        }
    }
}
