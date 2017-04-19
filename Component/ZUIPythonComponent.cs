using System.Linq;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SPEED;

namespace GhPython.Component
{


  [Guid("ed5912c8-6178-4140-8513-7b7a3e1e94ba")]
  public class ZuiPythonComponent : ScriptingAncestorComponent, IGH_VariableParameterComponent
  {

        protected override void AddDefaultInput(GH_InputParamManager pManager)
    {
            Param_ScriptVariable northInput = new Param_ScriptVariable();
            northInput.NickName = "north_";
            northInput.Name = "north_";
            northInput.Access = GH_ParamAccess.item;
            northInput.TypeHint = new GH_DoubleHint_CS();

            Param_ScriptVariable HBZones = new Param_ScriptVariable();
            HBZones.NickName = "_HBZones";
            HBZones.Name = "_HBZones";
            HBZones.Access = GH_ParamAccess.list;
            HBZones.TypeHint = new GH_BrepHint();

            Param_ScriptVariable HBContext = new Param_ScriptVariable();
            HBContext.NickName = "HBContext_";
            HBContext.Name = "HBContext_";
            HBContext.Access = GH_ParamAccess.list;
            HBContext.TypeHint = new GH_BrepHint();

            pManager.AddParameter(northInput);
            pManager.AddParameter(HBZones);
            pManager.AddParameter(HBContext);

    }

    protected override void AddDefaultOutput(GH_OutputParamManager pManager)
    {
    }

    internal override void FixGhInput(Param_ScriptVariable i, bool alsoSetIfNecessary = true)
    {
      i.Name = i.NickName;

      if (string.IsNullOrEmpty(i.Description))
        i.Description = string.Format("Script variable {0}", i.NickName);
      i.AllowTreeAccess = true;
      i.Optional = true;
      i.ShowHints = true;
      i.Hints = GetHints();

      if (alsoSetIfNecessary && i.TypeHint == null)
        i.TypeHint = i.Hints[1];
    }

    static readonly List<IGH_TypeHint> g_hints = new List<IGH_TypeHint>();
    static List<IGH_TypeHint> GetHints()
    {
      lock (g_hints)
      {
        if (g_hints.Count == 0)
        {
          g_hints.Add(new NoChangeHint());
          g_hints.Add(new GhDocGuidHint());

          g_hints.AddRange(PossibleHints);

          g_hints.RemoveAll(t =>
            {
              var y = t.GetType();
              return (y == typeof (GH_DoubleHint_CS) || y == typeof (GH_StringHint_CS));
            });
          g_hints.Insert(4, new NewFloatHint());
          g_hints.Insert(6, new NewStrHint());

          g_hints.Add(new GH_BoxHint());

          g_hints.Add(new GH_HintSeparator());

          g_hints.Add(new GH_LineHint());
          g_hints.Add(new GH_CircleHint());
          g_hints.Add(new GH_ArcHint());
          g_hints.Add(new GH_PolylineHint());

          g_hints.Add(new GH_HintSeparator());

          g_hints.Add(new GH_CurveHint());
          g_hints.Add(new GH_MeshHint());
          g_hints.Add(new GH_SurfaceHint());
          g_hints.Add(new GH_BrepHint());
          g_hints.Add(new GH_GeometryBaseHint());
        }
      }
      return g_hints;
    }

        #region IGH_VariableParameterComponent implementation


    IGH_Param ConstructVariable(GH_VarParamSide side, string nickname)
    {
        if (side == GH_VarParamSide.Input)
        {
            var param = new Param_ScriptVariable();
            if (!string.IsNullOrWhiteSpace(nickname))
                param.NickName = nickname;
            FixGhInput(param);
            return param;
        }
        if (side == GH_VarParamSide.Output)
        {
            var param = new Param_GenericObject();
            if (string.IsNullOrWhiteSpace(nickname))
                param.Name = param.NickName;
            else
            {
                param.NickName = nickname;
                param.Name = String.Format("Result {0}", nickname);
            }
            param.Description = String.Format("Output parameter {0}", param.NickName);
            return param;
        }
        return null;
    }

    public IGH_Param CreateParameter(GH_ParameterSide side, int index)
    {
      switch (side)
      {
        case GH_ParameterSide.Input:
          {
            return new Param_ScriptVariable
            {
                NickName = GH_ComponentParamServer.InventUniqueNickname("xyzuvwst", this.Params.Input),
                Name = NickName,
                Description = "Script variable " + NickName,
              };
          }
        case GH_ParameterSide.Output:
          {
            return new Param_GenericObject
              {
                NickName = GH_ComponentParamServer.InventUniqueNickname("abcdefghijklmn", this.Params.Output),
                Name = NickName,
                Description = "Script variable " + NickName,
              };
          }
        default:
          {
            return null;
          }
      }
    }

    bool IGH_VariableParameterComponent.DestroyParameter(GH_ParameterSide side, int index)
    {
      return true;
    }

    bool IGH_VariableParameterComponent.CanInsertParameter(GH_ParameterSide side, int index)
    {
            return false;
    }

    bool IGH_VariableParameterComponent.CanRemoveParameter(GH_ParameterSide side, int index)
    {
            return false;
    }

    protected override void SafeSolveInstance(IGH_DataAccess da)
    {

        SPEEDSuperClass.slidersConnectedToExportOSMComponent.Clear();

        GH_Document doc = OnPingDocument();

        //var watch = System.Diagnostics.Stopwatch.StartNew();

        foreach (SPEEDSlider slider in SPEEDSuperClass.existingSPEEDSliders)
        {
            if (doc.FindAllDownstreamObjects(slider).Contains(this))
            {
                if (slider.linkedCheckList != null)
                {
                    SPEEDSuperClass.slidersConnectedToExportOSMComponent.Add(slider);
                }

                if (slider.linkedCheckList == null)
                {
                    slider.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "You cannot generate a geometry option from this slider until a linked checklist is created, this slider will be ignored!");
                }
            }
        }

        //watch.Stop();

        SPEED.SPEEDSuperClass.updatedesignSpaceProfilers();

        SPEED.SPEEDSuperClass.updateDesignSpaceConstructors();

        if (SPEED.SPEEDSuperClass.slidersConnectedToExportOSMComponent.Count == 0)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "No SPEED sliders with linked CheckLists are connected! Only SPEED sliders can be used to form geometry for this component");

            return;
        }

        if (SPEEDSuperClass.slidersConnectedToExportOSMComponent.Count == 1)
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "One SPEED slider is connected");
        }
        else
        {
            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, SPEEDSuperClass.slidersConnectedToExportOSMComponent.Count.ToString() + " SPEED sliders are connected");
        }

        // Can only write OSM files IF SPEED Superclass canWriteOSMFile is set to true
        if (SPEED.SPEEDSuperClass.canWriteOSMFile == false)
        {
            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Component can only run through DesignSpace constructor, when the design space constructor is clicked");
            return;
        }

        // Set the current OSM File name so that the python code can read it 
        currentOSMFileName = SPEED.SPEEDSuperClass.currentOSMFileName;

        base.SafeSolveInstance(da);
    }

        public override void VariableParameterMaintenance()
    {
      foreach (Param_ScriptVariable variable in Params.Input.OfType<Param_ScriptVariable>())
        FixGhInput(variable);

      foreach (Param_GenericObject i in Params.Output.OfType<Param_GenericObject>())
      {
        i.Name = i.NickName;
        if (string.IsNullOrEmpty(i.Description))
          i.Description = i.NickName;
      }
    }

    protected override void SetScriptTransientGlobals()
    {
      base.SetScriptTransientGlobals();

      m_py.ScriptContextDoc = g_document;
      m_marshal = new NewComponentIOMarshal(g_document, this);
      m_py.SetVariable(DOCUMENT_NAME, g_document);
      m_py.SetIntellisenseVariable(DOCUMENT_NAME, g_document);
    }

    public override Guid ComponentGuid
    {
      get { return typeof(ZuiPythonComponent).GUID; }
    }

    #endregion
  }
}
