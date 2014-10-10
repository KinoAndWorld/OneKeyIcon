using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneKey
{
    public interface iConfigChange
    {
        void sizeConfigHasAdd(SizeConfig sc);
        void sizeConfigHasDeleted();
        void sizeConfigHasUpdate(SizeConfig sc, int changeIndex);
    }
}
