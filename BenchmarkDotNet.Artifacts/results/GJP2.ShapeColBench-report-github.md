```

BenchmarkDotNet v0.13.9+228a464e8be6c580ad9408e98f18813f6407fb5a, Windows 10 (10.0.19045.3570/22H2/2022Update)
Intel Core i3-2120 CPU 3.30GHz (Sandy Bridge), 1 CPU, 4 logical and 2 physical cores
.NET SDK 7.0.201
  [Host]     : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX
  DefaultJob : .NET 7.0.3 (7.0.323.6910), X64 RyuJIT AVX


```
| Method   | Mean     | Error   | StdDev  | Code Size |
|--------- |---------:|--------:|--------:|----------:|
| TestCol1 | 172.7 ns | 0.95 ns | 0.74 ns |     295 B |
