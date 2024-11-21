using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CK.Testing;

/// <summary>
/// Provides extension methods on <see cref="ILGenerator"/> class.
/// </summary>
static class ILGeneratorExtension
{
    /// <summary>
    /// Emits the IL to push (<see cref="OpCodes.Ldarg"/>) the actual argument at the given index onto the stack.
    /// </summary>
    /// <param name="g">This <see cref="ILGenerator"/> object.</param>
    /// <param name="i">Parameter index (0 being the 'this' for instance method).</param>
    public static void LdArg( this ILGenerator g, int i )
    {
        if( i == 0 ) g.Emit( OpCodes.Ldarg_0 );
        else if( i == 1 ) g.Emit( OpCodes.Ldarg_1 );
        else if( i == 2 ) g.Emit( OpCodes.Ldarg_2 );
        else if( i == 3 ) g.Emit( OpCodes.Ldarg_3 );
        else if( i < 255 ) g.Emit( OpCodes.Ldarg_S, (byte)i );
        else g.Emit( OpCodes.Ldarg, (short)i );
    }

    /// <summary>
    /// Emits the optimal IL to push the actual parameter values on the stack (<see cref="OpCodes.Ldarg_0"/>... <see cref="OpCodes.Ldarg"/>).
    /// </summary>
    /// <param name="g">This <see cref="ILGenerator"/> object.</param>
    /// <param name="startAtArgument0">False to skip the very first argument: for a method instance Arg0 is the 'this' object (see <see cref="System.Reflection.CallingConventions"/>) HasThis and ExplicitThis).</param>
    /// <param name="count">Number of parameters to push.</param>
    public static void RepushActualParameters( this ILGenerator g, bool startAtArgument0, int count )
    {
        if( count <= 0 ) return;
        if( startAtArgument0 )
        {
            g.Emit( OpCodes.Ldarg_0 );
            --count;
        }
        if( count > 0 )
        {
            g.Emit( OpCodes.Ldarg_1 );
            if( count > 1 )
            {
                g.Emit( OpCodes.Ldarg_2 );
                if( count > 2 )
                {
                    g.Emit( OpCodes.Ldarg_3 );
                    if( count > 3 )
                    {
                        for( int iParam = 4; iParam <= Math.Min( count, 255 ); ++iParam )
                        {
                            g.Emit( OpCodes.Ldarg_S, (byte)iParam );
                        }
                        if( count > 255 )
                        {
                            for( int iParam = 256; iParam <= count; ++iParam )
                            {
                                g.Emit( OpCodes.Ldarg, (short)iParam );
                            }
                        }
                    }
                }
            }
        }
    }
}
