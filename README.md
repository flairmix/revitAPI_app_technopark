# revitAPI_app_technopark

Плагин для Revit 2022 для объекта Технопарк. <br>
Инструменты для расчета теплопотерь и инструменты записи теплопотерь в модель.



---

requarements for install:
- .NET Framework 4.8 
- Revit 2022 
- Visual studio

---
Manifest for adding plugin in application. <br><br>
Create file and place in folder (change path to actual .dll):
```
C:\ProgramData\Autodesk\Revit\Addins\2022
```

<p align=right> revitAPI_app_technopark.addin </p>

```
<?xml version="1.0" encoding="utf-8" standalone="no"?>
<RevitAddIns>
    <AddIn Type="Application">
    <Name> Technopark Plug </Name>
    <Assembly> { PATH TO - helloWorld.dll } </Assembly>
    <AddInId>604b1052-f742-4951-8576-c261d1993108</AddInId>
    <FullClassName>NewPanelNamespace.CsAddPanel</FullClassName>
    <VendorId>ATP-TLP</VendorId>
    <VendorDescription> ATP-TLP (MID)</VendorDescription>
    </AddIn>
</RevitAddIns>
```

Build solution in visual studio and place in the same folder as Manifest

---
