using System;
using System.Collections.Generic;
using System.Text;
using monocat;

namespace client_emulation
{
    class WWWObserver : WWWManager
    {
        protected override void HandleError(WWWRequest request)
        {
            base.HandleError(request);

            Console.WriteLine("error:" + request.error);
        }
    }
}
