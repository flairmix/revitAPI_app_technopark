using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Technopark.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class FAQ : IExternalCommand
    {
        string textFAQ = "Для актуализации таблицы теплопотерь выполнить: " + Environment.NewLine +
            "1 - Находиться в модели ОТ " + Environment.NewLine +
            "2 - Включить следующие рабочие наборы и линк.модели:" + Environment.NewLine +
            "       - Пространства" + Environment.NewLine +
            "       - Архитектуру этажей, которые считаем" + Environment.NewLine +
            "       - Модель фасада - 164_TPS_Arch_GOR_EXT_A-C_Panel_(MEP)" + Environment.NewLine +
            "3 - Запустить кнопку 'Get Panels info'" + Environment.NewLine +
            "4 - Файлы с результатом перенести в папку проекта:" + Environment.NewLine +
            "       - \\03_Calculations\\08_Отопление и теплоснабжение\\MID_расчет теплопотерь\\{этаж} " + Environment.NewLine +
            "5 - Из ревита выгрузить спецификацию пространств 'SpaceLoadsScheduleHeating': " + Environment.NewLine +
            "       - при выгрузке установить разделитель 'точка с запятой - ;' " + Environment.NewLine +
            "       - оставить главные заголовки - headers, остальное все выключить " + Environment.NewLine +
            "6 - В папке с расчетами запустить '11899_расчет теплопотерь.exe' " + Environment.NewLine +
            "7 - На выходе получится csv файл, который можно загрузить в excel'  " + Environment.NewLine +
            "(ver_240823_0.20)";


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog info = new TaskDialog("Инфо");
            info.MainInstruction = "Как использовать это для расчета теплопотерь";
            info.MainContent = textFAQ;
            info.Show();

            return Result.Succeeded;
        }
    }
}
