using Omu.ValueInjecter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminApps.Helpers
{
    public class LoopValueInjectionIgnoreId : CustomizableValueInjection
    {
        protected Type TargetPropType;
        protected Type SourcePropType;

        protected virtual bool UseSourceProp(string sourcePropName)
        {
            if (sourcePropName == "ID")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        protected virtual string TargetPropName(string sourcePropName)
        {
            return sourcePropName;
        }

        protected override void Inject(object source, object target)
        {
            var sourceProps = source.GetProps();
            for (var i = 0; i < sourceProps.Count; i++)
            {
                var s = sourceProps[i];
                if (!UseSourceProp(s.Name)) continue;

                var t = target.GetProps().GetByName(SearchTargetName(TargetPropName(s.Name)));
                if (t == null) continue;
                if (!TypesMatch(s.PropertyType, t.PropertyType)) continue;
                TargetPropType = t.PropertyType;
                SourcePropType = s.PropertyType;
                var value = s.GetValue(source);
                if (AllowSetValue(value))
                    t.SetValue(target, SetValue(value));
            }
        }
    }
}