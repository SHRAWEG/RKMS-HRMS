using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hrms.Common.Helpers
{
    public static class NpoiHelper
    {
        public static HSSFCellStyle NormalCellStyle(HSSFWorkbook Workbook, HSSFFont Font)
        {
            HSSFCellStyle normalCellStyle = (HSSFCellStyle)Workbook.CreateCellStyle();
            normalCellStyle.SetFont(Font);
            normalCellStyle.VerticalAlignment = VerticalAlignment.Center;
            normalCellStyle.Alignment = HorizontalAlignment.Center;

            return normalCellStyle;
        }

        public static HSSFCellStyle BorderedCellStyle(HSSFWorkbook Workbook, HSSFFont Font)
        {
            HSSFCellStyle borderedCellStyle = (HSSFCellStyle)Workbook.CreateCellStyle();
            borderedCellStyle.SetFont(Font);
            borderedCellStyle.BorderLeft = BorderStyle.Medium;
            borderedCellStyle.BorderTop = BorderStyle.Medium;
            borderedCellStyle.BorderRight = BorderStyle.Medium;
            borderedCellStyle.BorderBottom = BorderStyle.Medium;
            borderedCellStyle.VerticalAlignment = VerticalAlignment.Center;
            borderedCellStyle.Alignment = HorizontalAlignment.Center;

            return borderedCellStyle;
        }

        public static void CreateCell(IRow CurrentRow, int CellIndex, object? Value, HSSFCellStyle Style)
        {
            ICell Cell = CurrentRow.CreateCell(CellIndex);

            if (Value is not null && Value.GetType() != typeof(string))
            {
                Cell.SetCellType(CellType.Numeric);
            }

            Cell.SetCellValue(Value?.ToString());
            Cell.CellStyle = Style;
        }
    }
}
