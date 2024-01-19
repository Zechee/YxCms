using System;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace ChuntianCms.Helper;

public class ExcelHelper
{

    #region 1.本地导出方法
    /// <summary>      
    /// DataTable导出到Excel文件      
    /// </summary>      
    /// <param name="dtSource">源DataTable</param>      
    /// <param name="strHeaderText">表头文本</param>      
    /// <param name="strFileName">保存位置</param>   
    /// <param name="strSheetName">工作表名称</param>   
    /// <Author>CallmeYhz 2015-11-26 10:13:09</Author>      
    public static void Export(DataTable dtSource, string strHeaderText = "", string strFileName = "", string strSheetName = "", string[] oldColumnNames = null, string[] newColumnNames = null)
    {
        if (strSheetName == "")
        {
            strSheetName = "Sheet";
        }
        MemoryStream getms = new MemoryStream();
        if (oldColumnNames.Length != newColumnNames.Length)
        {
            getms = new MemoryStream();
        }
        else
        {
            using (MemoryStream ms = Export(dtSource, strHeaderText, strSheetName, oldColumnNames, newColumnNames))
            {
                using (FileStream fs = new FileStream(strFileName, FileMode.Create, FileAccess.Write))
                {
                    byte[] data = ms.ToArray();
                    fs.Write(data, 0, data.Length);
                    fs.Flush();
                }
            }
        }
    }
    #endregion

    #region 0.导出公共方法DataTable导出到Excel的MemoryStream 
    /// <summary>      
    /// DataTable导出到Excel的MemoryStream      
    /// </summary>      
    /// <param name="dtSource">源DataTable</param>      
    /// <param name="strHeaderText">表头文本</param>      
    /// <param name="strSheetName">工作表名称</param>   
    /// <Author>CallmeYhz 2015-11-26 10:13:09</Author>      
    private static MemoryStream Export(DataTable dtSource, string strHeaderText = "", string strSheetName = "", string[] oldColumnNames = null, string[] newColumnNames = null)
    {
        if (oldColumnNames.Length != newColumnNames.Length)
        {
            return new MemoryStream();
        }
        var workbook = new HSSFWorkbook();
        //HSSFSheet sheet = workbook.CreateSheet();// workbook.CreateSheet();   
        var sheet = workbook.CreateSheet(strSheetName);
        #region 右击文件 属性信息
        {
            DocumentSummaryInformation dsi = PropertySetFactory.CreateDocumentSummaryInformation();
            dsi.Company = "http://www.abc.com/";
            workbook.DocumentSummaryInformation = dsi;
            SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
            si.Author = "雨田系统";//填加xls文件作者信息      
            si.ApplicationName = "NPOI";            //填加xls文件创建程序信息      
            si.LastAuthor = "雨田系统";           //填加xls文件最后保存者信息      
            si.Comments = "雨田系统";      //填加xls文件作者信息      
            si.Title = strHeaderText;               //填加xls文件标题信息      
            si.Subject = strHeaderText;              //填加文件主题信息      
            si.CreateDateTime = DateTime.Now;
            workbook.SummaryInformation = si;
        }
        #endregion
        ICellStyle dateStyle = workbook.CreateCellStyle();
        IDataFormat format = workbook.CreateDataFormat();
        dateStyle.DataFormat = format.GetFormat("yyyy-mm-dd");
        #region 取得列宽
        int[] arrColWidth = new int[oldColumnNames.Length];
        for (int i = 0; i < oldColumnNames.Length; i++)
        {
            arrColWidth[i] = Encoding.GetEncoding(936).GetBytes(newColumnNames[i]).Length;
        }
        /* 
        foreach (DataColumn item in dtSource.Columns) 
        { 
            arrColWidth[item.Ordinal] = Encoding.GetEncoding(936).GetBytes(item.ColumnName.ToString()).Length; 
        } 
         * */

        for (int i = 0; i < dtSource.Rows.Count; i++)
        {
            for (int j = 0; j < oldColumnNames.Length; j++)
            {
                int intTemp = Encoding.GetEncoding(936).GetBytes(dtSource.Rows[i][oldColumnNames[j]].ToString()).Length;
                if (intTemp > arrColWidth[j])
                {
                    arrColWidth[j] = intTemp;
                }
            }
            /* 
            for (int j = 0; j < dtSource.Columns.Count; j++) 
            { 
                int intTemp = Encoding.GetEncoding(936).GetBytes(dtSource.Rows[i][j].ToString()).Length; 
                if (intTemp > arrColWidth[j]) 
                { 
                    arrColWidth[j] = intTemp; 
                } 
            } 
             * */
        }
        #endregion
        int rowIndex = 0;

        foreach (DataRow row in dtSource.Rows)
        {
            #region 新建表，填充表头，填充列头，样式
            if (rowIndex == 65535 || rowIndex == 0)//一个页面最多承载6万多条数据
            {
                if (rowIndex != 0)
                {
                    sheet = workbook.CreateSheet(strSheetName + (rowIndex / 65535).ToString());
                }

                #region 表头及样式
                {
                    if (strHeaderText != "")
                    {
                        IRow headerRow = sheet.CreateRow(0);
                        headerRow.HeightInPoints = 25;
                        headerRow.CreateCell(0).SetCellValue(strHeaderText);

                        ICellStyle headStyle = workbook.CreateCellStyle();
                        headStyle.Alignment = HorizontalAlignment.Center;
                        IFont font = workbook.CreateFont();
                        font.FontHeightInPoints = 20;
                        font.IsBold = true;
                        headStyle.SetFont(font);

                        headerRow.GetCell(0).CellStyle = headStyle;
                        //sheet.AddMergedRegion(new Region(0, 0, 0, dtSource.Columns.Count - 1)); 
                        //合并单元格合并行，合并列
                        sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 0, 0, dtSource.Columns.Count - 1));
                    }

                }
                #endregion


                #region 列头及样式
                {
                    //HSSFRow headerRow = sheet.CreateRow(1); 
                    int rowStartNowIndex = strHeaderText != "" ? 1 : 0;
                    IRow headerRow = sheet.CreateRow(rowStartNowIndex);

                    ICellStyle headStyle = workbook.CreateCellStyle();
                    headStyle.Alignment = HorizontalAlignment.Center;
                    IFont font = workbook.CreateFont();
                    font.FontHeightInPoints = 10;
                    font.IsBold = true;
                    headStyle.SetFont(font);

                    for (int i = 0; i < oldColumnNames.Length; i++)
                    {
                        headerRow.CreateCell(i).SetCellValue(newColumnNames[i]);
                        headerRow.GetCell(i).CellStyle = headStyle;
                        //设置列宽   
                        sheet.SetColumnWidth(i, (arrColWidth[i] + 1) * 256);
                    }
                    /* 
                    foreach (DataColumn column in dtSource.Columns) 
                    { 
                        headerRow.CreateCell(column.Ordinal).SetCellValue(column.ColumnName); 
                        headerRow.GetCell(column.Ordinal).CellStyle = headStyle; 

                        //设置列宽    
                        sheet.SetColumnWidth(column.Ordinal, (arrColWidth[column.Ordinal] + 1) * 256); 
                    } 
                     * */
                }
                #endregion

                rowIndex = strHeaderText != "" ? 2 : 1;
            }
            #endregion
            #region 填充内容
            IRow dataRow = sheet.CreateRow(rowIndex);
            //foreach (DataColumn column in dtSource.Columns)   
            for (int i = 0; i < oldColumnNames.Length; i++)
            {
                ICell newCell = dataRow.CreateCell(i);
                string drValue = row[oldColumnNames[i]].ToString();
                switch (dtSource.Columns[oldColumnNames[i]].DataType.ToString())
                {
                    case "System.String"://字符串类型      
                        newCell.SetCellValue(drValue);
                        break;
                    case "System.DateTime"://日期类型      
                        DateTime dateV;
                        DateTime.TryParse(drValue, out dateV);
                        newCell.SetCellValue(dateV);
                        newCell.CellStyle = dateStyle;//格式化显示      
                        break;
                    case "System.Boolean"://布尔型      
                        bool boolV = false;
                        bool.TryParse(drValue, out boolV);
                        newCell.SetCellValue(boolV);
                        break;
                    case "System.Int16"://整型      
                    case "System.Int32":
                    case "System.Int64":
                    case "System.Byte":
                        int intV = 0;
                        int.TryParse(drValue, out intV);
                        newCell.SetCellValue(intV);
                        break;
                    case "System.Decimal"://浮点型      
                    case "System.Double":
                        double doubV = 0;
                        double.TryParse(drValue, out doubV);
                        newCell.SetCellValue(doubV);
                        break;
                    case "System.DBNull"://空值处理      
                        newCell.SetCellValue("");
                        break;
                    default:
                        newCell.SetCellValue("");
                        break;
                }
            }
            #endregion
            rowIndex++;
        }
        using (MemoryStream ms = new MemoryStream())
        {
            workbook.Write(ms);
            ms.Flush();
            ms.Position = 0;
            //sheet.Dispose();   
            sheet = null;
            workbook = null;
            //workbook.Dispose();//一般只用写这一个就OK了，他会遍历并释放所有资源，但当前版本有问题所以只释放sheet      
            return ms;
        }
    }
    #endregion

    #region 2.WEB导出方法带下载
    /// <summary>      
    /// WEB导出DataTable到Excel      
    /// </summary>      
    /// <param name="dtSource">源DataTable</param>      
    /// <param name="strHeaderText">表头文本</param>      
    /// <param name="strFileName">文件名</param>      
    /// <Author>CallmeYhz 2015-11-26 10:13:09</Author>      
    public static byte[] ExportByWeb(DataTable dtSource, string strHeaderText, string[] oldColumnNames, string[] newColumnNames, string strSheetName = "sheet")
    {
        if (oldColumnNames != null && newColumnNames != null)
        {
            return Export(dtSource, strHeaderText, strSheetName, oldColumnNames, newColumnNames).GetBuffer();
        }
        else
        {
            //生成列   
            string columns = "";
            for (int i = 0; i < dtSource.Columns.Count; i++)
            {
                if (i > 0)
                {
                    columns += ",";
                }
                columns += dtSource.Columns[i].ColumnName;
            }

            return Export(dtSource, strHeaderText, strSheetName, columns.Split(','), columns.Split(',')).GetBuffer();
        }
        //// 设置编码和附件格式      
        //context.Response.ContentType = "application/vnd.ms-excel";
        //context.Response.ContentEncoding = Encoding.UTF8;
        //context.Response.Charset = "";
        //context.Response.AppendHeader("Content-Disposition",
        //    "attachment;filename=" + HttpUtility.UrlEncode(strFileName, Encoding.UTF8));
        //if (oldColumnNames != null && newColumnNames != null)
        //{
        //    context.Response.BinaryWrite(Export(dtSource, strHeaderText, strSheetName, oldColumnNames, newColumnNames).GetBuffer());
        //    context.Response.End();
        //}
        //else
        //{
        //    //生成列   
        //    string columns = "";
        //    for (int i = 0; i < dtSource.Columns.Count; i++)
        //    {
        //        if (i > 0)
        //        {
        //            columns += ",";
        //        }
        //        columns += dtSource.Columns[i].ColumnName;
        //    }

        //    curContext.Response.BinaryWrite(Export(dtSource, strHeaderText, strSheetName, columns.Split(','), columns.Split(',')).GetBuffer());
        //    curContext.Response.End();
        //}
    }
    #endregion

    #region 1.简单导入方法读取excel表到内存中
    /// <summary>读取excel      
    /// 默认第一行为表头，导入第一个工作表   
    /// </summary>      
    /// <param name="strFileName">excel文档路径</param>      
    /// <returns></returns>      
    public static DataTable Import(string strFileName, bool isSample = false)
    {
        DataTable dt = new DataTable();
        IWorkbook hssfworkbook;
        using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
        {
            hssfworkbook = WorkbookFactory.Create(file);
        }
        ISheet sheet = hssfworkbook.GetSheetAt(0);
        System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
        IRow headerRow = sheet.GetRow(0);
        int cellCount = headerRow.LastCellNum;
        for (int j = 0; j < cellCount; j++)
        {
            ICell cell = headerRow.GetCell(j);
            dt.Columns.Add(cell.ToString());
        }
        for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++)
        {
            IRow row = sheet.GetRow(i);
            DataRow dataRow = dt.NewRow();

            for (int j = row.FirstCellNum; j < cellCount; j++)
            {
                //if (row.GetCell(j) != null)
                //    dataRow[j] = row.GetCell(j).ToString();
                try
                {
                    ICell cell = row.GetCell(j);
                    if (cell == null)
                    {
                        dataRow[j] = null;
                    }
                    else
                    {
                        if (isSample)
                        {
                            cell.SetCellType(CellType.String);
                            dataRow[j] = cell.StringCellValue;
                        }
                        else
                        {
                            //dataRow[j] = cell.ToString();   
                            string cellvalue = "";
                            switch (cell.CellType)
                            {
                                case CellType.Blank:
                                    dataRow[j] = null;
                                    break;
                                case CellType.Boolean:
                                    dataRow[j] = cell.BooleanCellValue;
                                    break;
                                case CellType.String:
                                    dataRow[j] = cell.StringCellValue;
                                    break;
                                case CellType.Error:
                                    dataRow[j] = cell.ErrorCellValue;
                                    break;
                                case CellType.Numeric:
                                case CellType.Formula:
                                    // 判断当前的Cell是否为Date
                                    if (DateUtil.IsCellDateFormatted(cell))
                                    {
                                        // 如果是在Date类型，则取得该Cell的Date值
                                        DateTime date = cell.DateCellValue;
                                        cellvalue = date.ToString();
                                        //区分Date  以及 DateTime
                                        if (cellvalue.Substring(cellvalue.IndexOf(" ") + 1).Equals("00:00:00"))
                                        {
                                            cellvalue = cellvalue.Substring(0, cellvalue.IndexOf(" "));
                                        }
                                    }
                                    else
                                    {
                                        //纯数字
                                        //区分整数与小数
                                        double aDouble = cell.NumericCellValue;
                                        cellvalue = aDouble + "";
                                    }
                                    dataRow[j] = cellvalue;
                                    break;
                                default:
                                    dataRow[j] = "=" + cell.CellFormula;
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }       
            }
            dt.Rows.Add(dataRow);
        }
        return dt;
    }

    public static DataTable Import(string strFileName, DataTable dt, bool isSample = false)
    {
       
        IWorkbook hssfworkbook;
        using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
        {
            hssfworkbook = WorkbookFactory.Create(file);
        }
        ISheet sheet = hssfworkbook.GetSheetAt(0);
        System.Collections.IEnumerator rows = sheet.GetRowEnumerator();
        IRow headerRow = sheet.GetRow(0);
        int cellCount = headerRow.LastCellNum;
        for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
        {
            IRow row = sheet.GetRow(i);
            DataRow dataRow = dt.NewRow();

            for (int j = row.FirstCellNum; j < cellCount; j++)
            {
                //if (row.GetCell(j) != null)
                //    dataRow[j] = row.GetCell(j).ToString();
                try
                {
                    ICell cell = row.GetCell(j);
                    if (cell == null)
                    {
                        dataRow[j] = null;
                    }
                    else
                    {
                        if (isSample)
                        {
                            cell.SetCellType(CellType.String);
                            dataRow[j] = cell.StringCellValue;
                        }
                        else
                        {
                            //dataRow[j] = cell.ToString();   
                            string cellvalue = "";
                            switch (cell.CellType)
                            {
                                case CellType.Blank:
                                    dataRow[j] = null;
                                    break;
                                case CellType.Boolean:
                                    dataRow[j] = cell.BooleanCellValue;
                                    break;
                                case CellType.String:
                                    dataRow[j] = cell.StringCellValue;
                                    break;
                                case CellType.Error:
                                    dataRow[j] = cell.ErrorCellValue;
                                    break;
                                case CellType.Numeric:
                                case CellType.Formula:
                                    // 判断当前的Cell是否为Date
                                    if (DateUtil.IsCellDateFormatted(cell))
                                    {
                                        // 如果是在Date类型，则取得该Cell的Date值
                                        DateTime date = cell.DateCellValue;
                                        cellvalue = date.ToString();
                                        //区分Date  以及 DateTime
                                        if (cellvalue.Substring(cellvalue.IndexOf(" ") + 1).Equals("00:00:00"))
                                        {
                                            cellvalue = cellvalue.Substring(0, cellvalue.IndexOf(" "));
                                        }
                                    }
                                    else
                                    {
                                        //纯数字
                                        //区分整数与小数
                                        double aDouble = cell.NumericCellValue;
                                        cellvalue = aDouble + "";
                                    }
                                    dataRow[j] = cellvalue;
                                    break;
                                default:
                                    dataRow[j] = "=" + cell.CellFormula;
                                    break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            dt.Rows.Add(dataRow);
        }
        return dt;
    }
    #endregion

    #region 2.精准定位导入方法从Excel中获取数据到DataTable
    /// <summary>   
    /// 从Excel中获取数据到DataTable   
    /// </summary>   
    /// <param name="strFileName">Excel文件全路径(服务器路径)</param>   
    /// <param name="SheetName">要获取数据的工作表名称</param>   
    /// <param name="HeaderRowIndex">工作表标题行所在行号(从0开始)</param>   
    /// <returns></returns>   
    public static DataTable Import(string strFileName, string SheetName, int HeaderRowIndex)
    {
        using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new HSSFWorkbook(file);
            ISheet sheet = workbook.GetSheet(SheetName);
            return RenderDataTableFromExcel(workbook, SheetName, HeaderRowIndex);
        }
    }

    /// <summary>   
    /// 从Excel中获取数据到DataTable   
    /// </summary>   
    /// <param name="strFileName">Excel文件全路径(服务器路径)</param>   
    /// <param name="SheetIndex">要获取数据的工作表序号(从0开始)</param>   
    /// <param name="HeaderRowIndex">工作表标题行所在行号(从0开始)</param>   
    /// <returns></returns>   
    public static DataTable Import(string strFileName, int SheetIndex, int HeaderRowIndex, bool isSample = false)
    {
        using (FileStream file = new FileStream(strFileName, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new HSSFWorkbook(file);
            string SheetName = workbook.GetSheetName(SheetIndex);
            return RenderDataTableFromExcel(workbook, SheetName, HeaderRowIndex, isSample);
        }
    }

    /// <summary>   
    /// 从Excel中获取数据到DataTable   
    /// </summary>   
    /// <param name="ExcelFileStream">Excel文件流</param>   
    /// <param name="SheetName">要获取数据的工作表名称</param>   
    /// <param name="HeaderRowIndex">工作表标题行所在行号(从0开始)</param>   
    /// <returns></returns>   
    public static DataTable Import(Stream ExcelFileStream, string SheetName, int HeaderRowIndex)
    {
        IWorkbook workbook = new HSSFWorkbook(ExcelFileStream);
        ExcelFileStream.Close();
        return RenderDataTableFromExcel(workbook, SheetName, HeaderRowIndex);
    }

    /// <summary>   
    /// 从Excel中获取数据到DataTable   
    /// </summary>   
    /// <param name="ExcelFileStream">Excel文件流</param>   
    /// <param name="SheetIndex">要获取数据的工作表序号(从0开始)</param>   
    /// <param name="HeaderRowIndex">工作表标题行所在行号(从0开始)</param>   
    /// <returns></returns>   
    public static DataTable Import(Stream ExcelFileStream, int SheetIndex, int HeaderRowIndex)
    {
        IWorkbook workbook = new HSSFWorkbook(ExcelFileStream);
        ExcelFileStream.Close();
        string SheetName = workbook.GetSheetName(SheetIndex);
        return RenderDataTableFromExcel(workbook, SheetName, HeaderRowIndex);
    }
    #endregion

    #region 0.公共导入方法从Excel中获取数据到DataTable
    /// <summary>   
    /// 从Excel中获取数据到DataTable   
    /// </summary>   
    /// <param name="workbook">要处理的工作薄</param>   
    /// <param name="SheetName">要获取数据的工作表名称</param>   
    /// <param name="HeaderRowIndex">工作表标题行所在行号(从0开始)</param>   
    /// <returns></returns>   
    private static DataTable RenderDataTableFromExcel(IWorkbook workbook, string SheetName, int HeaderRowIndex, bool isSample = false)
    {
        ISheet sheet = workbook.GetSheet(SheetName);
        DataTable table = new DataTable();
        try
        {
            IRow headerRow = sheet.GetRow(HeaderRowIndex);
            int cellCount = headerRow.LastCellNum;
            //添加列
            for (int i = headerRow.FirstCellNum; i < cellCount; i++)
            {
                if (headerRow.GetCell(i) != null)
                {
                    headerRow.GetCell(i).SetCellType(CellType.String);
                    DataColumn column = new DataColumn(headerRow.GetCell(i).StringCellValue);
                    table.Columns.Add(column);
                }

            }
            //int rowCount = sheet.LastRowNum;
            #region 循环各行各列,写入数据到DataTable
            for (int i = (sheet.FirstRowNum + HeaderRowIndex + 1); i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                DataRow dataRow = table.NewRow();
                for (int j = row.FirstCellNum; j < cellCount; j++)
                {
                    ICell cell = row.GetCell(j);
                    if (cell == null)
                    {
                        dataRow[j] = null;
                    }
                    else
                    {
                        if (isSample)
                        {
                            cell.SetCellType(CellType.String);
                            dataRow[j] = cell.StringCellValue;
                        }
                        else
                        {
                            //dataRow[j] = cell.ToString();   
                            switch (cell.CellType)
                            {
                                case CellType.Blank:
                                    dataRow[j] = null;
                                    break;
                                case CellType.Boolean:
                                    dataRow[j] = cell.BooleanCellValue;
                                    break;
                                case CellType.Numeric:
                                    dataRow[j] = cell.NumericCellValue;
                                    break;
                                case CellType.String:
                                    dataRow[j] = cell.StringCellValue;
                                    break;
                                case CellType.Error:
                                    dataRow[j] = cell.ErrorCellValue;
                                    break;
                                case CellType.Formula:
                                default:
                                    dataRow[j] = "=" + cell.CellFormula;
                                    break;
                            }
                        }
                    }
                }
                table.Rows.Add(dataRow);
                //dataRow[j] = row.GetCell(j).ToString();   
            }
            #endregion
        }
        catch (System.Exception ex)
        {
            table.Clear();
            table.Columns.Clear();
            table.Columns.Add("出错了");
            DataRow dr = table.NewRow();
            dr[0] = ex.Message;
            table.Rows.Add(dr);
            return table;
        }
        finally
        {
            //sheet.Dispose();   
            workbook = null;
            sheet = null;
        }
        #region 清除最后的空行
        for (int i = table.Rows.Count - 1; i > 0; i--)
        {
            bool isnull = true;
            for (int j = 0; j < table.Columns.Count; j++)
            {
                if (table.Rows[i][j] != null || table.Rows[i][j].ToString() != "")
                {
                    isnull = false;
                    break;
                }
            }
            if (isnull)
            {
                table.Rows[i].Delete();
            }
        }
        #endregion
        return table;
    }
    #endregion
}

