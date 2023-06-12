using nxtlvlOS.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IL2CPU.API.Attribs;
using IL2CPU.API;
using XSharp;
using XSharp.Assembler;
using nxtlvlOS.Windowing.Utils;

namespace nxtlvlOS.Plugs {
    [Plug(Target = typeof(ColorUtils))]
    public class ColorUtilsImpl {
        [PlugMethod(Assembler = typeof(AlphaBlendAssembler))]
        public static uint AlphaBlend(uint original, uint target) {
            throw new NotImplementedException();
        }

        public class AlphaBlendAssembler : AssemblerMethod {
            public override void AssembleNew(Assembler aAssembler, object aMethodInfo) {
                new LiteralAssemblerCode(@"
mov     DWORD PTR [rbp-20], edi
        mov     DWORD PTR [rbp-24], esi
        mov     eax, DWORD PTR [rbp-20]
        shr     eax, 24
        mov     BYTE PTR [rbp-1], al
        mov     eax, DWORD PTR [rbp-24]
        shr     eax, 24
        mov     BYTE PTR [rbp-2], al
        mov     eax, DWORD PTR [rbp-20]
        shr     eax, 16
        movzx   edx, al
        movzx   eax, BYTE PTR [rbp-1]
        mov     ecx, edx
        imul    ecx, eax
        mov     eax, DWORD PTR [rbp-24]
        shr     eax, 16
        movzx   eax, al
        movzx   edx, BYTE PTR [rbp-1]
        mov     esi, 255
        sub     esi, edx
        mov     edx, esi
        imul    eax, edx
        add     eax, ecx
        shr     eax, 8
        mov     BYTE PTR [rbp-3], al
        mov     eax, DWORD PTR [rbp-20]
        shr     eax, 8
        movzx   edx, al
        movzx   eax, BYTE PTR [rbp-1]
        mov     ecx, edx
        imul    ecx, eax
        mov     eax, DWORD PTR [rbp-24]
        shr     eax, 8
        movzx   eax, al
        movzx   edx, BYTE PTR [rbp-1]
        mov     esi, 255
        sub     esi, edx
        mov     edx, esi
        imul    eax, edx
        add     eax, ecx
        shr     eax, 8
        mov     BYTE PTR [rbp-4], al
        mov     eax, DWORD PTR [rbp-20]
        movzx   edx, al
        movzx   eax, BYTE PTR [rbp-1]
        mov     ecx, edx
        imul    ecx, eax
        mov     eax, DWORD PTR [rbp-24]
        movzx   eax, al
        movzx   edx, BYTE PTR [rbp-1]
        mov     esi, 255
        sub     esi, edx
        mov     edx, esi
        imul    eax, edx
        add     eax, ecx
        shr     eax, 8
        mov     BYTE PTR [rbp-5], al
        movzx   eax, BYTE PTR [rbp-3]
        sal     eax, 16
        lea     edx, [rax-16777216]
        movzx   eax, BYTE PTR [rbp-4]
        sal     eax, 8
        add     edx, eax
        movzx   eax, BYTE PTR [rbp-5]
        add     eax, edx");
            }
        }
    }
}
