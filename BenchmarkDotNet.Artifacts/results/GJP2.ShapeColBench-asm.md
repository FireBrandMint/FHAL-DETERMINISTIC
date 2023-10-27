## .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX
```assembly
; GJP2.ShapeColBench.TestCol1()
       sub       rsp,48
       vxorps    xmm4,xmm4,xmm4
       vmovdqa   xmmword ptr [rsp+20],xmm4
       vmovdqa   xmmword ptr [rsp+30],xmm4
       xor       eax,eax
       mov       [rsp+40],rax
       xor       ecx,ecx
       mov       [rsp+28],rcx
       mov       [rsp+30],rcx
       mov       [rsp+38],rcx
       mov       [rsp+40],rcx
       mov       rcx,286C3806618
       mov       rcx,[rcx]
       mov       rdx,286C3806620
       mov       rdx,[rdx]
       lea       r8,[rsp+20]
       cmp       [rcx],ecx
       call      qword ptr [7FFA50EFB390]; GJP2.Shape.IntersectsInfo(GJP2.Shape, GJP2.CollisionResult ByRef)
       nop
       add       rsp,48
       ret
; Total bytes of code 94
```
```assembly
; GJP2.Shape.IntersectsInfo(GJP2.Shape, GJP2.CollisionResult ByRef)
       push      rdi
       push      rsi
       sub       rsp,28
       mov       rsi,r8
       movsx     rdi,word ptr [rcx+2C]
       mov       r8d,edi
       movsx     rax,word ptr [rdx+2C]
       add       r8d,eax
       movsx     r8,r8w
       cmp       r8d,2
       je        short M01_L00
       cmp       r8d,9
       je        short M01_L01
       cmp       r8d,10
       jne       near ptr M01_L03
       mov       r8,rsi
       call      qword ptr [7FFA50EFB450]
       jmp       near ptr M01_L03
M01_L00:
       mov       r8,rsi
       call      qword ptr [7FFA50EFB420]; GJP2.Shape.ConvexConvexIntersectsInfo(GJP2.Shape, GJP2.CollisionResult ByRef)
       jmp       short M01_L03
M01_L01:
       cmp       edi,1
       jne       short M01_L02
       mov       r8,rsi
       call      qword ptr [7FFA50EFB438]
       jmp       short M01_L03
M01_L02:
       mov       [rsp+40],rcx
       mov       rcx,rdx
       mov       rdx,[rsp+40]
       mov       r8,rsi
       call      qword ptr [7FFA50EFB438]
       lea       rax,[rsi+8]
       mov       rdx,[rax]
       mov       rcx,[rax+8]
       neg       rdx
       shl       rdx,0C
       sar       rdx,0C
       neg       rcx
       shl       rcx,0C
       sar       rcx,0C
       mov       [rax],rdx
       mov       [rax+8],rcx
       add       rsi,18
       mov       rax,[rsi]
       mov       rdx,[rsi+8]
       neg       rax
       shl       rax,0C
       sar       rax,0C
       neg       rdx
       shl       rdx,0C
       sar       rdx,0C
       mov       [rsi],rax
       mov       [rsi+8],rdx
M01_L03:
       add       rsp,28
       pop       rsi
       pop       rdi
       ret
; Total bytes of code 201
```

