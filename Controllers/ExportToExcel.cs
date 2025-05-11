using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ETMF.Models;
using Microsoft.AspNetCore.Mvc;

namespace ETMF.Controllers
{
    // DocumentsController的部分类扩展，用于实现Excel导出功能
    public partial class DocumentsController
    {
        private IActionResult ExportToExcel(List<Document> documents)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("文档列表");

            // 设置标题行样式
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;

            // 添加标题行
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "标题";
            worksheet.Cell(1, 3).Value = "描述";
            worksheet.Cell(1, 4).Value = "文档类型";
            worksheet.Cell(1, 5).Value = "路径";
            worksheet.Cell(1, 6).Value = "状态";
            worksheet.Cell(1, 7).Value = "作者";
            worksheet.Cell(1, 8).Value = "文件版本";
            worksheet.Cell(1, 9).Value = "创建日期";
            worksheet.Cell(1, 10).Value = "最后更新日期";

            // 添加数据行
            int row = 2;
            foreach (var doc in documents)
            {
                string artifactName = "未指定";
                string path = "";

                if (doc.Artifact != null)
                {
                    artifactName = doc.Artifact.Name;

                    if (doc.Artifact.Section != null)
                    {
                        path += doc.Artifact.Section.Name;

                        if (doc.Artifact.Section.Zone != null)
                        {
                            path = $"{doc.Artifact.Section.Zone.Number}.{doc.Artifact.Section.Number} - {doc.Artifact.Section.Zone.Name}/{doc.Artifact.Section.Name}";
                        }
                    }
                }

                int versionCount = doc.Versions?.Count ?? 0;

                worksheet.Cell(row, 1).Value = doc.Id;
                worksheet.Cell(row, 2).Value = doc.Title;
                worksheet.Cell(row, 3).Value = doc.Description;
                worksheet.Cell(row, 4).Value = artifactName;
                worksheet.Cell(row, 5).Value = path;
                worksheet.Cell(row, 6).Value = doc.Status;
                worksheet.Cell(row, 7).Value = doc.Author;
                worksheet.Cell(row, 8).Value = versionCount;
                worksheet.Cell(row, 9).Value = doc.CreatedAt;
                worksheet.Cell(row, 10).Value = doc.UpdatedAt;

                // 格式化日期列
                worksheet.Cell(row, 9).Style.DateFormat.Format = "yyyy-MM-dd";
                if (doc.UpdatedAt.HasValue)
                {
                    worksheet.Cell(row, 10).Style.DateFormat.Format = "yyyy-MM-dd";
                }

                row++;
            }

            // 自动调整列宽
            worksheet.Columns().AdjustToContents();

            // 导出为Excel文件
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"documents_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
} 