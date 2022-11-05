using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPITrainingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitAPITrainingСreateElementsAndAnnotations
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public Pipe Pipe { get; }

        public List<FamilySymbol> Tags { get; }

        public FamilySymbol SelectedFamilyType { get; set; }

        public FamilySymbol SelectedTagType { get; set; }

        public List<WallType> WallTypes { get; } = new List<WallType>();

        public List<DuctType> DuctTypes { get; } = new List<DuctType>();

        public List<Level> Levels { get; } = new List<Level>();

        public DelegateCommand SaveCommand { get; }

        public WallType SelectedWallType { get; set; }

        public MEPSystemType DuctSystemTypes { get; set; }

        public DuctType SelectedDuctTypes { get; set; }

        //public MEPSystemType DuctTypes { get; } = new MEPSystemType;

        public Level SelectedLevel { get; set; }

        public double WallHeight { get; set; }

        public List<XYZ> Points { get; } = new List<XYZ>();

        public MainViewViewModel(ExternalCommandData commandData) 
        {
            _commandData = commandData;
            //Tags = TagsUtils.GetPipeTagTypes(commandData);
            //Pipe = SelectionUtils.GetObject<Pipe>(commandData, "Выберите трубу");
            //FamilyTypes = FamilySymbolUtils.GetFamilySymbols(commandData);
            //WallTypes = WallsUtils.GetWallTypes(commandData);
            DuctSystemTypes = DuctUtils.GetDuctSystemTypes(commandData);
            Levels = LevelsUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(OnSaveCommand);
            WallHeight = 100;
            Points = SelectionUtils.GetPoints(_commandData, "Выберите точки", ObjectSnapTypes.Endpoints);
        }

        private void OnSaveCommand() 
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            //var locationCurve = Pipe.Location as LocationCurve;
            //var pipeCurve = locationCurve.Curve;
            //var pipeMidPoint = (pipeCurve.GetEndPoint(0) + pipeCurve.GetEndPoint(1)) / 2;

            //using (var ts = new Transaction(doc, "Create tag"))
            //{
            //    ts.Start();
            //    IndependentTag.Create(doc, SelectedTagType.Id, doc.ActiveView.Id, new Reference(Pipe), false, TagOrientation.Horizontal, pipeMidPoint);
            //    ts.Commit();
            //}


            //var oLevel = (Level)doc.GetElement(Pipe.LevelId);

            //FamilyInstanceUtils.CreateFamilyInstance(_commandData, SelectedFamilyType, pipeCurve.GetEndPoint(0), oLevel);
            //FamilyInstanceUtils.CreateFamilyInstance(_commandData, SelectedFamilyType, pipeCurve.GetEndPoint(1), oLevel);

            //ReiseCloseRequest();

            if (Points.Count > 2 || DuctSystemTypes == null || SelectedLevel == null || SelectedDuctTypes == null)
                return;

            var curves = new List<Curve>();

            for (int i = 0; i < Points.Count; i++)
            {
                if (i == 0)
                    continue;

                var prevPoint = Points[i - 1];
                var currentPoint = Points[i];

                Curve curve = Line.CreateBound(prevPoint, currentPoint);
                curves.Add(curve);
            }

            using (var ts = new Transaction(doc, "Create Duct"))
            {
                ts.Start();

                foreach (var curve in curves)
                {
                        Duct.Create(doc, DuctSystemTypes.Id, SelectedDuctTypes.Id, SelectedLevel.Id,
                        PickPoint, currentPoint);

                }

                ts.Commit();
            }

        }
        public event EventHandler CloseRequest;
        private void ReiseCloseRequest() 
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
