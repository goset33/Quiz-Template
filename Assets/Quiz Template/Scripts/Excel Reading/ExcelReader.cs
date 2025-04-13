using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// [TODO]: Тут нужно обрабатывать дополнительные столбцы для вопроса 4
public static class ExcelReader
{
    public static string[] GetAllSheetNames(string path)
    {
        using FileStream file = new FileStream(path, FileMode.Open, FileAccess.Read);

        XSSFWorkbook workbook = new(file);
        List<string> sheets = new();
        for (int i = 0; i < workbook.NumberOfSheets; i++)
        {
            sheets.Add(workbook.GetSheetName(i));
        }
        return sheets.ToArray();
    }

    // Название таблицы должно точно совпадать с ее реальной копией в файле, с точностью до регистра
    public static List<Dictionary<string, string>> ReadSheet(string filePath, string sheetName)
    {
        List<Dictionary<string, string>> sheetData = new List<Dictionary<string, string>>();
        using FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        XSSFWorkbook workbook = new XSSFWorkbook(file);
        XSSFSheet sheet = workbook.GetSheet(sheetName) as XSSFSheet ?? throw new Exception($"Таблица '{sheetName}' не найдена");
        IRow headerRow = sheet.GetRow(0);
        for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            IRow row = sheet.GetRow(rowIndex);
            if (IsRowEmpty(row)) continue;

            Dictionary<string, string> rowData = new Dictionary<string, string>();
            for (int columnIndex = 0; columnIndex < headerRow.LastCellNum; columnIndex++) // Обработка строки по столбцам
            {
                string header = headerRow.GetCell(columnIndex)?.ToString().Trim() ?? $"Column-{columnIndex}";
                ICell cell = row.GetCell(columnIndex);
                if (cell == null)
                {
                    rowData[header] = string.Empty;
                }

                else if (headerRow.GetCell(columnIndex).ToString() == "Изображение" && cell is XSSFCell xssfCell) // Обработка изображений
                {
                    var pictureData = GetPictureFromCell(xssfCell);
                    if (pictureData != null)
                    {
                        string base64Image = Convert.ToBase64String(pictureData);
                        rowData[header] = base64Image;
                    }
                    else
                    {
                        rowData[header] = string.Empty;
                        Debug.LogWarning("Изображение не найдено!");
                    }
                }
                else
                {
                    try
                    {
                        rowData[header] = cell.StringCellValue;
                    }
                    catch
                    {
                        rowData[header] = cell.NumericCellValue.ToString();
                    }
                }
            }
            sheetData.Add(rowData);
        }
        return sheetData;
    }

    // Метод достает из нужной клетки изображение и возвращает его массив байтов
    private static byte[] GetPictureFromCell(XSSFCell cell)
    {
        XSSFSheet sheet = cell.Sheet as XSSFSheet;
        if (sheet == null) return null;

        XSSFDrawing drawing = sheet.GetDrawingPatriarch();
        if (drawing == null) return null;
        Debug.Log($"Найдено изображений: {drawing.GetShapes().Count}");

        foreach (XSSFShape shape in drawing.GetShapes())
        {
            if (shape is XSSFPicture picture)
            {
                IClientAnchor anchor = picture.ClientAnchor;
                if (anchor.Row1 == cell.RowIndex && anchor.Col1 == cell.ColumnIndex)
                {
                    return picture.PictureData.Data;
                }
            }
        }
        return null;
    }

    // Костыль, проверяющий строку. Если хоть в одной клетке что-то есть то возвращает false
    // А костыль потому, что библиотека говно. Если в строке раньше что-то было то теперь она учитывается в цикле строк
    private static bool IsRowEmpty(IRow row)
    {
        if (row == null) return true;

        for (int i = row.FirstCellNum; i < row.LastCellNum; i++)
        {
            ICell cell = row.GetCell(i);
            if (cell != null && cell.CellType != CellType.Blank && cell.CellType != CellType.Unknown)
            {
                return false;
            }
        }
        return true;
    }
}
