using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.Testing
{

    class ExplicitTestManager
    {
        enum KeyCode : int
        {
            Control = 0x11,
            Shift = 0x10,
        }

        const int KeyPressedMask = 0x8000;

        [System.Runtime.InteropServices.DllImport( "user32.dll" )]
        static extern short GetKeyState( int key );

        static bool _callFailed;
        static bool IsKeyDown( KeyCode key ) => (GetKeyState( (int)key ) & KeyPressedMask) != 0;

        public static bool IsExplicitAllowed
        {
            get
            {
                if( !_callFailed )
                {
                    try
                    {
                        return IsKeyDown( KeyCode.Control );
                    }
                    catch
                    {
                        _callFailed = true;
                    }
                }
                return false;
            }
         }
    }
}
