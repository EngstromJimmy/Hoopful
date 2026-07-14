using System;
using System.Collections.Generic;
using System.Text;

namespace hexreader.EmbroideryHandlers
{
    class EmbroideryHandlerFactrory
    {

        public static IEmbroideryHandler GetEmbroideryHandler(string Path)
        {
            switch (System.IO.Path.GetExtension(Path).ToLower())
            {
                case ".pcs":
                    return new EmbroideryHandlers.PCSHandler();
                case ".hus":
                    return new EmbroideryHandlers.HUSHandler();
                case ".exp":
                    return new EmbroideryHandlers.EXPHandler();
                case ".jef":
                    return new EmbroideryHandlers.JEFHandler();
                case ".sew":
                    return new EmbroideryHandlers.SEWHandler();
                case ".ksm":
                    return new EmbroideryHandlers.KSMHandler();
                case ".dst":
                    return new EmbroideryHandlers.DSTHandler();
                case ".vip":
                    return new EmbroideryHandlers.VIPHandler();
                case ".vp3":
                    return new EmbroideryHandlers.VP3Handler();
                case ".pes":
                   // MessageBox.Show("This Feature is not implemented yet");
                    return new EmbroideryHandlers.PESHandler(); ;
                case ".pec":
                    // MessageBox.Show("This Feature is not implemented yet");
                    return new EmbroideryHandlers.PECHandler(); ;
                case ".xxx":
                    //MessageBox.Show("This Feature is not fully implemented yet");
                    return new EmbroideryHandlers.XXXHandler();
                default:
                    return null;
            }
        }
    }
}
