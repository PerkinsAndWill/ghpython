﻿using System;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Rhino.Geometry;
using System.Runtime.InteropServices;

namespace GhPython.Component
{
  [Guid("C1C11093-4F61-4E99-90C7-113C6421CC73")]
  class DynamicHint : GH_NullHint, IGH_TypeHint
  {
    private readonly PythonComponent_OBSOLETE m_component;

    public DynamicHint(PythonComponent_OBSOLETE component)
    {
      if (component == null)
        throw new ArgumentNullException("component");

      m_component = component;
    }

    Guid IGH_TypeHint.HintID { get { return GetType().GUID; } }

    string IGH_TypeHint.TypeName { get { return "dynamic"; } }

    bool IGH_TypeHint.Cast(object data, out object target)
    {
      bool toReturn = base.Cast(data, out target);

      if (m_component.DocStorageMode == DocReplacement.DocStorage.AutomaticMarshal && target != null)
      {
        Type t = target.GetType();

        if (t == typeof (Line))
          target = new LineCurve((Line) target);

        else if (t == typeof (Arc))
          target = new ArcCurve((Arc) target);

        else if (t == typeof (Circle))
          target = new ArcCurve((Circle) target);

        else if (t == typeof (Ellipse))
          target = ((Ellipse) target).ToNurbsCurve();

        else if (t == typeof (Box))
          target = Brep.CreateFromBox((Box) target);

        else if (t == typeof (BoundingBox))
          target = Brep.CreateFromBox((BoundingBox) target);

        else if (t == typeof (Rectangle3d))
          target = ((Rectangle3d) target).ToNurbsCurve();

        else if (t == typeof (Polyline))
          target = new PolylineCurve((Polyline) target);
      }

      return toReturn;
    }
  }
}