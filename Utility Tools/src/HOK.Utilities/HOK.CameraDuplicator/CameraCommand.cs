﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HOK.CameraDuplicator
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    public class CameraCommand:IExternalCommand
    {
        private UIApplication m_app = null;

        Result IExternalCommand.Execute(ExternalCommandData commandData, ref string message, Autodesk.Revit.DB.ElementSet elements)
        {
            m_app = commandData.Application;
            if (m_app.Application.Documents.Size > 1)
            {
                CameraWindow cameraWindow = new CameraWindow(m_app);
                if (true == cameraWindow.ShowDialog())
                {
                    cameraWindow.Close();
                }
            }
            else
            {
                MessageBox.Show("Please open more than two Revit documents before running this tool.\n A source model and a recipient model are required.", "Opened Revit Documents Required!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return Result.Succeeded;
        }
    }

    public class CameraViewInfo
    {
        private int viewId = -1;
        private string viewName = "";
        
        private int linkedViewId = -1;
        private bool linked = false;

        private ElementId viewTemplateId = ElementId.InvalidElementId;
        private ElementId phaseId = ElementId.InvalidElementId;
        private Dictionary<WorksetId, WorksetVisibility> worksetVisibilities = new Dictionary<WorksetId, WorksetVisibility>();
        
        private ViewOrientation3D orientation = null;
        private bool isCropBoxOn = false;
        private BoundingBoxXYZ cropBox = null;
        private bool isSectionBoxOn = false;
        private BoundingBoxXYZ sectionBox = null;
        private DisplayStyle display = DisplayStyle.Undefined;
       
        private Dictionary<string, Parameter> viewParameters = new Dictionary<string, Parameter>();
        private bool isSelected = false;
        
        public int ViewId { get { return viewId; } set { viewId = value; } }
        public string ViewName { get { return viewName; } set { viewName = value; } }

        public int LinkedViewId { get { return linkedViewId; } set { linkedViewId = value; } }
        public bool Linked { get { return linked; } set { linked = value; } }

        public ElementId ViewTemplateId { get { return viewTemplateId; } set { viewTemplateId = value; } }
        public ElementId PhaseId { get { return phaseId; } set { phaseId = value; } }
        public Dictionary<WorksetId, WorksetVisibility> WorksetVisibilities { get { return worksetVisibilities; } set { worksetVisibilities = value; } }

        public ViewOrientation3D Orientation { get { return orientation; } set { orientation = value; } }
        public bool IsCropBoxOn { get { return isCropBoxOn; } set { isCropBoxOn = value; } }
        public BoundingBoxXYZ CropBox { get { return cropBox; } set { cropBox = value; } }
        public bool IsSectionBoxOn { get { return isSectionBoxOn; } set { isSectionBoxOn = value; } }
        public BoundingBoxXYZ SectionBox { get { return sectionBox; } set { sectionBox = value; } }
        public DisplayStyle Display { get { return display; } set { display = value; } }
        public Dictionary<string, Parameter> ViewParameters { get { return viewParameters; } set { viewParameters = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

        public CameraViewInfo(View3D view3d)
        {
            viewId = view3d.Id.IntegerValue;
            viewName = view3d.ViewName;

            viewTemplateId = view3d.ViewTemplateId;
            
            orientation = view3d.GetOrientation();
            isCropBoxOn = view3d.CropBoxActive;
            cropBox = view3d.CropBox;
#if RELEASE2013
            sectionBox = view3d.SectionBox;
#elif RELEASE2014||RELEASE2015 || RELEASE2016
            isSectionBoxOn = view3d.IsSectionBoxActive;
            sectionBox = view3d.GetSectionBox();
#endif

            display = view3d.DisplayStyle;
            GetParameters(view3d.Parameters);
        }

        public CameraViewInfo(CameraViewInfo vInfo)
        {
            this.ViewId = vInfo.ViewId;
            this.ViewName = vInfo.ViewName;
            this.LinkedViewId = vInfo.LinkedViewId;
            this.Linked = vInfo.Linked;
            this.ViewTemplateId = vInfo.ViewTemplateId;
            this.PhaseId = vInfo.PhaseId;
            this.WorksetVisibilities = vInfo.WorksetVisibilities;
            this.Orientation = vInfo.Orientation;
            this.CropBox = vInfo.CropBox;
            this.Display = vInfo.Display;
            this.ViewParameters = vInfo.ViewParameters;
            this.IsSelected = vInfo.IsSelected;
        }

        private void GetParameters(ParameterSet parameters)
        {
            try
            {
                foreach (Parameter param in parameters)
                {
                    string paramName = param.Definition.Name;
                    if (paramName.Contains(".Extensions")) { continue; }
                    if (param.Id.IntegerValue == (int)BuiltInParameter.VIEW_PHASE) { phaseId = param.AsElementId(); }
                    if (param.StorageType == StorageType.None || param.StorageType  == StorageType.ElementId) { continue; }

                    if (!viewParameters.ContainsKey(paramName) && param.HasValue)
                    {
                        viewParameters.Add(paramName, param);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void GetWorksetVisibilities(View3D view3d, List<WorksetId> worksetIds)
        {
            try
            {
                foreach (WorksetId wsId in worksetIds)
                {
                    if (!worksetVisibilities.ContainsKey(wsId))
                    {
                        worksetVisibilities.Add(wsId, view3d.GetWorksetVisibility(wsId));
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }

    public class PlanViewInfo
    {
        private int viewId = -1;
        private string viewName = "";
        private ViewType planViewType = ViewType.Undefined;

        private int linkedViewId = -1;
        private bool linked = false;

        private ElementId levelId = ElementId.InvalidElementId;
        private string levelName = "";

        private ElementId viewTemplateId = ElementId.InvalidElementId;
        private ElementId scopeBoxId = ElementId.InvalidElementId;
        private ElementId phaseId = ElementId.InvalidElementId;
        private ElementId areaShcemeId = ElementId.InvalidElementId;
        private Dictionary<WorksetId, WorksetVisibility> worksetVisibilities = new Dictionary<WorksetId, WorksetVisibility>();

        private bool isCropBoxOn = false;
        private BoundingBoxXYZ cropBox = null;
        private DisplayStyle display = DisplayStyle.Undefined;

        private Dictionary<string, Parameter> viewParameters = new Dictionary<string, Parameter>();
        private bool isSelected = false;

        public int ViewId { get { return viewId; } set { viewId = value; } }
        public string ViewName { get { return viewName; } set { viewName = value; } }
        public ViewType PlanViewType { get { return planViewType; } set { planViewType = value; } }

        public int LinkedViewId { get { return linkedViewId; } set { linkedViewId = value; } }
        public bool Linked { get { return linked; } set { linked = value; } }

        public ElementId LevelId { get { return levelId; } set { levelId = value; } }
        public string LevelName { get { return levelName; } set { levelName = value; } }

        public ElementId ViewTemplateId { get { return viewTemplateId; } set { viewTemplateId = value; } }
        public ElementId ScopeBoxId { get { return scopeBoxId; } set { scopeBoxId = value; } }
        public ElementId PhaseId { get { return phaseId; } set { phaseId = value; } }
        public ElementId AreaSchemeId { get { return areaShcemeId; } set { areaShcemeId = value; } }
        public Dictionary<WorksetId, WorksetVisibility> WorksetVisibilities { get { return worksetVisibilities; } set { worksetVisibilities = value; } }

        public bool IsCropBoxOn { get { return isCropBoxOn; } set { isCropBoxOn = value; } }
        public BoundingBoxXYZ CropBox { get { return cropBox; } set { cropBox = value; } }
        public DisplayStyle Display { get { return display; } set { display = value; } }

        public Dictionary<string, Parameter> ViewParameters { get { return viewParameters; } set { viewParameters = value; } }
        public bool IsSelected { get { return isSelected; } set { isSelected = value; } }

        public PlanViewInfo(ViewPlan planView)
        {
            viewId = planView.Id.IntegerValue;
            viewName = planView.ViewName;
            planViewType = planView.ViewType;

            if (null != planView.GenLevel)
            {
                levelId = planView.GenLevel.Id;
                levelName = planView.GenLevel.Name;
            }
            
            viewTemplateId = planView.ViewTemplateId;
            if (null != planView.AreaScheme)
            {
                areaShcemeId = planView.AreaScheme.Id;
            }

            isCropBoxOn = planView.CropBoxActive;
            cropBox = planView.CropBox;

            display = planView.DisplayStyle;
            GetParameters(planView.Parameters);
        }

        public PlanViewInfo(PlanViewInfo info)
        {
            this.ViewId = info.ViewId;
            this.ViewName = info.ViewName;
            this.PlanViewType = info.PlanViewType;
            this.LinkedViewId = info.LinkedViewId;
            this.Linked = info.Linked;
            this.LevelId = info.LevelId;
            this.LevelName = info.LevelName;
            this.ViewTemplateId = info.ViewTemplateId;
            this.ScopeBoxId = info.ScopeBoxId;
            this.PhaseId = info.PhaseId;
            this.AreaSchemeId = info.AreaSchemeId; 
            this.IsCropBoxOn = info.IsCropBoxOn;
            this.CropBox = info.CropBox;
            this.Display = info.Display;
            this.viewParameters = info.ViewParameters;
            this.IsSelected = info.IsSelected;
        }

        private void GetParameters(ParameterSet parameters)
        {
            try
            {
                foreach (Parameter param in parameters)
                {
                    string paramName = param.Definition.Name;
                    if (paramName.Contains(".Extensions")) { continue; }
                    if (param.Id.IntegerValue == (int)BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP) { scopeBoxId = param.AsElementId(); continue; }
                    if (param.Id.IntegerValue == (int)BuiltInParameter.VIEW_PHASE) { phaseId = param.AsElementId(); continue; }
                    if (param.StorageType == StorageType.None || param.StorageType == StorageType.ElementId) { continue; }
                    if (!viewParameters.ContainsKey(paramName) && param.HasValue)
                    {
                        viewParameters.Add(paramName, param);
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }

        public void GetWorksetVisibilities(ViewPlan viewPlan, List<WorksetId> worksetIds)
        {
            try
            {
                foreach (WorksetId wsId in worksetIds)
                {
                    if (!worksetVisibilities.ContainsKey(wsId))
                    {
                        worksetVisibilities.Add(wsId, viewPlan.GetWorksetVisibility(wsId));
                    }
                }
            }
            catch (Exception ex)
            {
                string message = ex.Message;
            }
        }
    }

    public class ModelInfo
    {
        private string modelId = "";
        private string modelName = "";
        private Document modelDoc = null;
        private List<WorksetId> worksetIds = new List<WorksetId>();
        private Dictionary<int/*viewId*/, CameraViewInfo> cameraViews = new Dictionary<int, CameraViewInfo>();
        private Dictionary<int/*viewId*/, PlanViewInfo> planViews = new Dictionary<int, PlanViewInfo>();

        public string ModelId { get { return modelId; } set { modelId = value; } }
        public string ModelName { get { return modelName; } set { modelName = value; } }
        public Document ModelDoc { get { return modelDoc; } set { modelDoc = value; } }
        public Dictionary<int, CameraViewInfo> CameraViews { get { return cameraViews; } set { cameraViews = value; } }
        public Dictionary<int, PlanViewInfo> PlanViews { get { return planViews; } set { planViews = value; } }

        public ModelInfo(Document doc, string id)
        {
            modelDoc = doc;
            modelName = doc.Title;
            modelId = id;

            if (modelDoc.IsWorkshared)
            {
                GetWorksets();
            }
            GetCameraViews();
            GetPlanViews();
        }

        private void GetWorksets()
        {
            try
            {
                FilteredWorksetCollector wsCollector = new FilteredWorksetCollector(modelDoc);
                WorksetKindFilter wsFilter = new WorksetKindFilter(WorksetKind.UserWorkset);
                worksetIds = wsCollector.WherePasses(wsFilter).ToWorksetIds().ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get worksets.\n"+ex.Message, "Get Worksets", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetCameraViews()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(modelDoc);
                List<View3D> threeDViews = collector.OfClass(typeof(View3D)).ToElements().Cast<View3D>().ToList();
                foreach (View3D view in threeDViews)
                {
                    if (view.ViewType != ViewType.ThreeD) { continue; }
                    if (view.IsTemplate) { continue; }
                    if (view.IsPerspective)
                    {
                        CameraViewInfo viewInfo = new CameraViewInfo(view);
                        if (worksetIds.Count > 0)
                        {
                            viewInfo.GetWorksetVisibilities(view, worksetIds);
                        }
                        if (!cameraViews.ContainsKey(viewInfo.ViewId))
                        {
                            cameraViews.Add(viewInfo.ViewId, viewInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get camera views.\n" + ex.Message, "Get Camera Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void GetPlanViews()
        {
            try
            {
                FilteredElementCollector collector = new FilteredElementCollector(modelDoc);
                List<ViewPlan> viewPlans = collector.OfClass(typeof(ViewPlan)).ToElements().Cast<ViewPlan>().ToList();
                foreach (ViewPlan pView in viewPlans)
                {
                    if (pView.IsTemplate) { continue; }
                    Parameter parentParam = pView.get_Parameter(BuiltInParameter.SECTION_PARENT_VIEW_NAME);
                    if (null != parentParam)
                    {
                        if (parentParam.HasValue) { continue; }
                    }

                    if (!planViews.ContainsKey(pView.Id.IntegerValue))
                    {
                        PlanViewInfo pvi = new PlanViewInfo(pView);
                        if (worksetIds.Count > 0)
                        {
                            pvi.GetWorksetVisibilities(pView, worksetIds);
                        }
                        planViews.Add(pvi.ViewId, pvi);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to get plan views.\n"+ex.Message, "Get Plan Views", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class ViewConfiguration
    {
        private bool applyWorksetVisibility = false;
        private List<MapItemInfo> mapItems = new List<MapItemInfo>();

        public bool ApplyWorksetVisibility { get { return applyWorksetVisibility; } set { applyWorksetVisibility = value; } }
        public List<MapItemInfo> MapItems { get { return mapItems; } set { mapItems = value; } }

        public ViewConfiguration()
        {
        }

        public ViewConfiguration(bool apply, List<MapItemInfo> maps)
        {
            applyWorksetVisibility = apply;
            mapItems = maps;
        }
    }

    public class MapItemInfo
    {
        private string sourceModelId = "";
        private string recipientModelId = "";
        private MapType mapItemType = MapType.None;
        private int sourceItemId = -1;
        private int recipientItemId = -1;
        private string sourceItemName = "";
        private string recipientItemName = "";

        public string SourceModelId { get { return sourceModelId; } set { sourceModelId = value; } }
        public string RecipientModelId { get { return recipientModelId; } set { recipientModelId = value; } }
        public MapType MapItemType { get { return mapItemType; } set { mapItemType = value; } }
        public int SourceItemId { get { return sourceItemId; } set { sourceItemId = value; } }
        public int RecipientItemId { get { return recipientItemId; } set { recipientItemId = value; } }
        public string SourceItemName { get { return sourceItemName; } set { sourceItemName = value; } }
        public string RecipientItemName { get { return recipientItemName; } set { recipientItemName = value; } }

        public MapItemInfo()
        {
        }
    }

    public enum MapType
    {
        Level, ViewTemplate, ScopeBox, Workset, Phase, AreaScheme, None
    }

    public class ViewTypeInfo
    {
        private string viewTypeName = "";
        private ViewType viewTypeEnum = ViewType.Undefined;
        private ViewFamily viewFamilyEnum = ViewFamily.ThreeDimensional;

        public string ViewTypeName { get { return viewTypeName; } set { viewTypeName = value; } }
        public ViewType ViewTypeEnum { get { return viewTypeEnum; } set { viewTypeEnum = value; } }
        public ViewFamily ViewFamilyEnum { get { return viewFamilyEnum; } set { viewFamilyEnum = value; } }

        public ViewTypeInfo()
        {
        }

        public ViewTypeInfo(string name, ViewType vType, ViewFamily vFamily)
        {
            viewTypeName = name;
            viewTypeEnum = vType;
            viewFamilyEnum = vFamily;
        }
    }
}
