using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.VisualStudio.ProjectSystem.VS.Build
{
    internal class DesignTimeBuildErrorTableEntry : ITableEntry
    {
        public Object Identity
        {
            get
            {
                return null;
            }
        }

        public Boolean CanSetValue(String keyName)
        {
            return false;
        }

        public Boolean TryGetValue(String keyName, out Object content)
        {
            content = null;
            return false;
        }

        public Boolean TrySetValue(String keyName, Object content)
        {
            return false;
        }
    }
}
